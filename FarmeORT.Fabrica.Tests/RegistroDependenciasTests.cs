using FarmeORT.IServicios.Finalizaciones;
using FarmeORT.IServicios.Inscripciones;
using FarmeORT.IServicios.Ranking;
using FarmeORT.IServicios.Repertorio;
using FarmeORT.IServicios.Rondas;
using FarmeORT.IServicios.Sesiones;
using FarmeORT.IServicios.Skills;
using FarmeORT.IServicios.Torneos;
using FarmeORT.IServicios.Usuarios;
using Microsoft.Extensions.DependencyInjection;

namespace FarmeORT.Fabrica.Tests;

[TestClass]
public sealed class RegistroDependenciasTests
{
    private const string ConnectionString = "Server=servidor-de-prueba;Database=FarmeORT;Trusted_Connection=True;";

    [TestMethod]
    [DataRow(typeof(IServicioSesiones))]
    [DataRow(typeof(IServicioUsuarios))]
    [DataRow(typeof(IServicioSkills))]
    [DataRow(typeof(IServicioTorneos))]
    [DataRow(typeof(IServicioRepertorio))]
    [DataRow(typeof(IServicioSkillsObligatorias))]
    [DataRow(typeof(IServicioInscripciones))]
    [DataRow(typeof(IServicioRanking))]
    [DataRow(typeof(IServicioRondas))]
    [DataRow(typeof(IServicioFinalizaciones))]
    public void AgregarDependencias_ServicioQueUsaLaApi_SeResuelveConLifetimesValidos(Type servicio)
    {
        var servicios = new ServiceCollection();

        servicios.AgregarDependencias(ConnectionString);

        var opciones = new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true };
        using var proveedor = servicios.BuildServiceProvider(opciones);
        using var scope = proveedor.CreateScope();
        Assert.IsNotNull(scope.ServiceProvider.GetService(servicio));
    }
}
