using FarmeORT.Excepciones;
using FarmeORT.IRepositorios.Usuarios;
using FarmeORT.IServicios.Seguridad;
using FarmeORT.Servicios.Usuarios;
using Moq;

namespace FarmeORT.Servicios.Tests.Usuarios;

[TestClass]
public sealed class ServicioEliminacionUsuariosTests
{
    private const int AdministradorActualId = 1;

    private Mock<IRepositorioUsuarios> _repositorioUsuarios = null!;
    private ServicioUsuarios _servicio = null!;

    [TestInitialize]
    public void Inicializar()
    {
        _repositorioUsuarios = new Mock<IRepositorioUsuarios>(MockBehavior.Strict);
        var hasheadorContrasenas = new Mock<IHasheadorContrasenas>(MockBehavior.Strict);
        var reloj = new Mock<TimeProvider>(MockBehavior.Strict);
        _servicio = new ServicioUsuarios(_repositorioUsuarios.Object, hasheadorContrasenas.Object, reloj.Object);
    }

    [TestMethod]
    public void Eliminar_UsuarioActual_LanzaOperacionSobreSiMismoSinConsultarLaBase()
    {
        Assert.ThrowsExactly<OperacionSobreSiMismoException>(() => _servicio.Eliminar(AdministradorActualId, AdministradorActualId));
    }
}
