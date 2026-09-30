using FarmeORT.Dominio.Usuarios;
using FarmeORT.Repositorios.Usuarios;

namespace FarmeORT.Repositorios.Tests.Usuarios;

[TestClass]
public sealed class RepositorioUsuariosTests
{
    private const string Email = "jugador@farmeort.com";

    private BaseDeDatosEnMemoria _baseDeDatos = null!;
    private RepositorioUsuarios _repositorio = null!;

    [TestInitialize]
    public void Inicializar()
    {
        _baseDeDatos = new BaseDeDatosEnMemoria();
        _repositorio = new RepositorioUsuarios(_baseDeDatos.Contexto);
    }

    [TestCleanup]
    public void Limpiar()
    {
        _baseDeDatos.Dispose();
    }

    [TestMethod]
    public void ObtenerPorEmail_EmailExistente_DevuelveUsuario()
    {
        var otroUsuario = new Usuario { Email = "otro@farmeort.com", Rol = Rol.Organizador, ContrasenaHash = "hash-otro" };
        var usuario = new Usuario { Email = Email, Rol = Rol.Jugador, ContrasenaHash = "hash-jugador" };
        _baseDeDatos.Contexto.Usuarios.AddRange(otroUsuario, usuario);
        _baseDeDatos.Contexto.SaveChanges();
        _baseDeDatos.Contexto.ChangeTracker.Clear();

        var resultado = _repositorio.ObtenerPorEmail(Email);

        Assert.IsNotNull(resultado);
        Assert.AreEqual(usuario.Id, resultado.Id);
        Assert.AreEqual(Email, resultado.Email);
        Assert.AreEqual(Rol.Jugador, resultado.Rol);
        Assert.AreEqual("hash-jugador", resultado.ContrasenaHash);
    }

    [TestMethod]
    public void ObtenerPorEmail_AdministradorPrecargado_DevuelveAdministrador()
    {
        var resultado = _repositorio.ObtenerPorEmail("admin@farmeort.com");

        Assert.IsNotNull(resultado);
        Assert.AreEqual(Rol.Administrador, resultado.Rol);
        Assert.IsFalse(string.IsNullOrEmpty(resultado.ContrasenaHash));
    }

    [TestMethod]
    public void ObtenerPorEmail_EmailInexistente_DevuelveNull()
    {
        var otroUsuario = new Usuario { Email = "otro@farmeort.com", Rol = Rol.Organizador, ContrasenaHash = "hash-otro" };
        _baseDeDatos.Contexto.Usuarios.Add(otroUsuario);
        _baseDeDatos.Contexto.SaveChanges();

        var resultado = _repositorio.ObtenerPorEmail(Email);

        Assert.IsNull(resultado);
    }
}
