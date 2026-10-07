using FarmeORT.Dominio.Batallas;
using FarmeORT.IServicios.Batallas;
using FarmeORT.Servicios.Batallas;
using Microsoft.Extensions.DependencyInjection;

namespace FarmeORT.Servicios.Tests.Batallas;

[TestClass]
public sealed class RegistroBatallasTests
{
    [TestMethod]
    [DataRow(typeof(ISorteo), typeof(SorteoAleatorio))]
    [DataRow(typeof(IServicioLanzamientos), typeof(ServicioLanzamientos))]
    public void AgregarServicios_ContratoDeBatallas_RegistraImplementacionScoped(Type interfaz, Type implementacion)
    {
        var servicios = new ServiceCollection();

        servicios.AgregarServicios();

        var registro = servicios.SingleOrDefault(r => r.ServiceType == interfaz);
        Assert.IsNotNull(registro);
        Assert.AreEqual(implementacion, registro.ImplementationType);
        Assert.AreEqual(ServiceLifetime.Scoped, registro.Lifetime);
    }
}
