using System.Text.Json;
using FarmeORT.DTOs.Errores;
using FarmeORT.WebAPI.Filtros;
using FarmeORT.WebAPI.Validaciones;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FarmeORT.WebAPI.Tests;

[TestClass]
public sealed class RegistroWebApiTests
{
    [TestMethod]
    public void AgregarControllersConFiltros_FiltroGlobal_RegistraElFiltroDeExcepciones()
    {
        var servicios = new ServiceCollection().AddLogging();

        servicios.AgregarControllersConFiltros();

        using var proveedor = servicios.BuildServiceProvider();
        var opciones = proveedor.GetRequiredService<IOptions<MvcOptions>>().Value;
        var filtros = opciones.Filters.OfType<TypeFilterAttribute>().Select(f => f.ImplementationType);
        CollectionAssert.Contains(filtros.ToList(), typeof(FiltroExcepciones));
    }

    [TestMethod]
    public void AgregarControllersConFiltros_DatosInvalidos_UsanLaRespuestaDeValidacionDelEquipo()
    {
        var servicios = new ServiceCollection().AddLogging();

        servicios.AgregarControllersConFiltros();

        using var proveedor = servicios.BuildServiceProvider();
        var opciones = proveedor.GetRequiredService<IOptions<ApiBehaviorOptions>>().Value;
        Assert.AreEqual<Func<ActionContext, IActionResult>>(RespuestaValidacion.Crear, opciones.InvalidModelStateResponseFactory);
    }

    // Si MVC convierte sus errores en ProblemDetails (en inglés), el 415 nunca llega sin cuerpo al manejador del equipo.
    [TestMethod]
    public void AgregarControllersConFiltros_ErroresDeClienteDeMvc_NoLosConvierteEnProblemDetails()
    {
        var servicios = new ServiceCollection().AddLogging();

        servicios.AgregarControllersConFiltros();

        using var proveedor = servicios.BuildServiceProvider();
        var opciones = proveedor.GetRequiredService<IOptions<ApiBehaviorOptions>>().Value;
        Assert.IsTrue(opciones.SuppressMapClientErrors);
    }

    // Un tipo mal formado en la query (limit=abc) sigue siendo 400, pero el detalle lo ve el usuario: en español.
    [TestMethod]
    public void AgregarControllersConFiltros_ValorQueNoSeConvierte_LoExplicaEnEspanol()
    {
        var servicios = new ServiceCollection().AddLogging();

        servicios.AgregarControllersConFiltros();

        using var proveedor = servicios.BuildServiceProvider();
        var mensajes = proveedor.GetRequiredService<IOptions<MvcOptions>>().Value.ModelBindingMessageProvider;
        Assert.AreEqual("El valor 'abc' no es válido.", mensajes.AttemptedValueIsInvalidAccessor("abc", "Limit"));
    }

    // Una excepción en un filtro de autenticación no pasa por FiltroExcepciones: no puede llegar al cliente.
    [TestMethod]
    public async Task UsarManejoDeErrores_ExcepcionFueraDelController_Devuelve500SinDetallesInternos()
    {
        await using var aplicacion = WebApplication.CreateBuilder().Build();
        aplicacion.UsarManejoDeErrores();
        aplicacion.Run(_ => throw new InvalidOperationException("Cannot open database FarmeORT"));
        var contexto = CrearContexto(aplicacion);

        await ((IApplicationBuilder)aplicacion).Build()(contexto);

        Assert.AreEqual(StatusCodes.Status500InternalServerError, contexto.Response.StatusCode);
        var error = LeerError(contexto);
        Assert.AreEqual("INTERNAL_ERROR", error.ErrorCode);
        Assert.AreEqual("Ocurrió un error inesperado.", error.UserMessage);
    }

    // El enrutamiento y MVC responden algunos errores sin cuerpo (ruta inexistente, método no admitido, sin JSON).
    [TestMethod]
    [DataRow(StatusCodes.Status404NotFound, "NOT_FOUND", "No existe el recurso solicitado.")]
    [DataRow(StatusCodes.Status405MethodNotAllowed, "METHOD_NOT_ALLOWED", "El recurso no admite esa operación.")]
    [DataRow(StatusCodes.Status415UnsupportedMediaType, "UNSUPPORTED_MEDIA_TYPE", "El cuerpo tiene que enviarse en formato JSON.")]
    public async Task UsarManejoDeErrores_ErrorSinCuerpo_LeAgregaElErrorDelEquipo(int status, string codigo, string mensaje)
    {
        await using var aplicacion = WebApplication.CreateBuilder().Build();
        aplicacion.UsarManejoDeErrores();
        aplicacion.Run(peticion =>
        {
            peticion.Response.StatusCode = status;
            return Task.CompletedTask;
        });
        var contexto = CrearContexto(aplicacion);

        await ((IApplicationBuilder)aplicacion).Build()(contexto);

        Assert.AreEqual(status, contexto.Response.StatusCode);
        var error = LeerError(contexto);
        Assert.AreEqual(codigo, error.ErrorCode);
        Assert.AreEqual(mensaje, error.UserMessage);
    }

    [TestMethod]
    public async Task UsarManejoDeErrores_ErrorSinCuerpoNoPrevisto_ConservaElEstadoConUnErrorGenerico()
    {
        await using var aplicacion = WebApplication.CreateBuilder().Build();
        aplicacion.UsarManejoDeErrores();
        aplicacion.Run(peticion =>
        {
            peticion.Response.StatusCode = StatusCodes.Status406NotAcceptable;
            return Task.CompletedTask;
        });
        var contexto = CrearContexto(aplicacion);

        await ((IApplicationBuilder)aplicacion).Build()(contexto);

        Assert.AreEqual(StatusCodes.Status406NotAcceptable, contexto.Response.StatusCode);
        var error = LeerError(contexto);
        Assert.AreEqual("REQUEST_NOT_PROCESSED", error.ErrorCode);
        Assert.AreEqual("No se pudo procesar la solicitud.", error.UserMessage);
    }

    private static DefaultHttpContext CrearContexto(WebApplication aplicacion)
    {
        var contexto = new DefaultHttpContext { RequestServices = aplicacion.Services };
        contexto.Response.Body = new MemoryStream();
        return contexto;
    }

    private static ErrorResponse LeerError(HttpContext contexto)
    {
        Assert.IsGreaterThan(0L, contexto.Response.Body.Length, "La respuesta de error no tiene cuerpo.");
        contexto.Response.Body.Position = 0;
        var error = JsonSerializer.Deserialize<ErrorResponse>(contexto.Response.Body, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        Assert.IsNotNull(error);
        return error;
    }
}
