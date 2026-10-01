using FarmeORT.IServicios.Seguridad;
using FarmeORT.IServicios.Sesiones;
using FarmeORT.IServicios.Skills;
using FarmeORT.IServicios.Usuarios;
using FarmeORT.Servicios.Seguridad;
using FarmeORT.Servicios.Sesiones;
using FarmeORT.Servicios.Skills;
using FarmeORT.Servicios.Usuarios;
using Microsoft.Extensions.DependencyInjection;

namespace FarmeORT.Servicios.Tests;

[TestClass]
public sealed class RegistroServiciosTests
{
    [TestMethod]
    [DataRow(typeof(IServicioSesiones), typeof(ServicioSesiones))]
    [DataRow(typeof(IHasheadorContrasenas), typeof(HasheadorContrasenas))]
    [DataRow(typeof(IGeneradorTokens), typeof(GeneradorTokens))]
    [DataRow(typeof(IServicioUsuarios), typeof(ServicioUsuarios))]
    [DataRow(typeof(IServicioSkills), typeof(ServicioSkills))]
    public void AgregarServicios_InterfazDeServicio_RegistraImplementacionScoped(Type interfaz, Type implementacion)
    {
        var servicios = new ServiceCollection();

        servicios.AgregarServicios();

        var registro = servicios.SingleOrDefault(r => r.ServiceType == interfaz);
        Assert.IsNotNull(registro);
        Assert.AreEqual(implementacion, registro.ImplementationType);
        Assert.AreEqual(ServiceLifetime.Scoped, registro.Lifetime);
    }
}
