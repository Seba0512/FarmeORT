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

    private static DefaultHttpContext CrearContexto(WebApplication aplicacion)
    {
        var contexto = new DefaultHttpContext { RequestServices = aplicacion.Services };
        contexto.Response.Body = new MemoryStream();
        return contexto;
    }

    private static ErrorResponse LeerError(HttpContext contexto)
    {
        contexto.Response.Body.Position = 0;
        var error = JsonSerializer.Deserialize<ErrorResponse>(contexto.Response.Body, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        Assert.IsNotNull(error);
        return error;
    }
}
