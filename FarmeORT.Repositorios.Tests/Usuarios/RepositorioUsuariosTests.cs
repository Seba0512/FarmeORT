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
        var otroUsuario = CrearUsuario("otro@farmeort.com", Rol.Organizador, "hash-otro");
        var usuario = CrearUsuario(Email, Rol.Jugador, "hash-jugador");
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
        var otroUsuario = CrearUsuario("otro@farmeort.com", Rol.Organizador, "hash-otro");
        _baseDeDatos.Contexto.Usuarios.Add(otroUsuario);
        _baseDeDatos.Contexto.SaveChanges();

        var resultado = _repositorio.ObtenerPorEmail(Email);

        Assert.IsNull(resultado);
    }

    private static Usuario CrearUsuario(string email, Rol rol, string contrasenaHash)
    {
        var nickname = email.Split('@')[0];
        return new Usuario("Nombre", "Apellido", email, nickname, new DateOnly(2000, 1, 15), rol, contrasenaHash);
    }
}
