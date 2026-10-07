using FarmeORT.IRepositorios.Batallas;
using FarmeORT.Repositorios.Batallas;
using Microsoft.Extensions.DependencyInjection;

namespace FarmeORT.Repositorios.Tests.Batallas;

[TestClass]
public sealed class RegistroBatallasTests
{
    [TestMethod]
    public void AgregarRepositorios_Batallas_RegistraImplementacionScoped()
    {
        var servicios = new ServiceCollection();

        servicios.AgregarRepositorios("Server=servidor-de-prueba;Database=FarmeORT;Trusted_Connection=True;");

        var registro = servicios.SingleOrDefault(r => r.ServiceType == typeof(IRepositorioBatallas));
        Assert.IsNotNull(registro);
        Assert.AreEqual(typeof(RepositorioBatallas), registro.ImplementationType);
        Assert.AreEqual(ServiceLifetime.Scoped, registro.Lifetime);
    }
}
