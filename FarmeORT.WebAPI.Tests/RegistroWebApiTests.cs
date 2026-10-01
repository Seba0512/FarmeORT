using FarmeORT.WebAPI.Filtros;
using FarmeORT.WebAPI.Validaciones;
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
}
