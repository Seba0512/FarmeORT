using FarmeORT.IRepositorios.Sesiones;
using FarmeORT.IRepositorios.Usuarios;
using FarmeORT.Repositorios.Sesiones;
using FarmeORT.Repositorios.Usuarios;
using Microsoft.Extensions.DependencyInjection;

namespace FarmeORT.Repositorios.Tests;

[TestClass]
public sealed class RegistroRepositoriosTests
{
    private const string ConnectionString = "Server=servidor-de-prueba;Database=FarmeORT;Trusted_Connection=True;";

    [TestMethod]
    [DataRow(typeof(IRepositorioUsuarios), typeof(RepositorioUsuarios))]
    [DataRow(typeof(IRepositorioSesiones), typeof(RepositorioSesiones))]
    [DataRow(typeof(FarmeORTContext), typeof(FarmeORTContext))]
    public void AgregarRepositorios_TipoDeAccesoADatos_RegistraImplementacionScoped(Type tipo, Type implementacion)
    {
        var servicios = new ServiceCollection();

        servicios.AgregarRepositorios(ConnectionString);

        var registro = servicios.SingleOrDefault(r => r.ServiceType == tipo);
        Assert.IsNotNull(registro);
        Assert.AreEqual(implementacion, registro.ImplementationType);
        Assert.AreEqual(ServiceLifetime.Scoped, registro.Lifetime);
    }
}
