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

    [TestMethod]
    public void ObtenerPorId_IdExistente_DevuelveElUsuario()
    {
        var otroUsuario = CrearUsuario("otro@farmeort.com", Rol.Organizador, "hash-otro");
        var usuario = CrearUsuario(Email, Rol.Jugador, "hash-jugador");
        _baseDeDatos.Contexto.Usuarios.AddRange(otroUsuario, usuario);
        _baseDeDatos.Contexto.SaveChanges();
        _baseDeDatos.Contexto.ChangeTracker.Clear();

        var resultado = _repositorio.ObtenerPorId(usuario.Id);

        Assert.IsNotNull(resultado);
        Assert.AreEqual(usuario.Id, resultado.Id);
        Assert.AreEqual(Email, resultado.Email);
        Assert.AreEqual(Rol.Jugador, resultado.Rol);
    }

    [TestMethod]
    public void ObtenerPorId_IdInexistente_DevuelveNull()
    {
        var resultado = _repositorio.ObtenerPorId(99);

        Assert.IsNull(resultado);
    }

    [TestMethod]
    public void ExistePorNickname_NicknameExistente_DevuelveTrue()
    {
        _baseDeDatos.Contexto.Usuarios.Add(CrearUsuario(Email, Rol.Jugador, "hash-jugador"));
        _baseDeDatos.Contexto.SaveChanges();
        _baseDeDatos.Contexto.ChangeTracker.Clear();

        var existe = _repositorio.ExistePorNickname("jugador");

        Assert.IsTrue(existe);
    }

    [TestMethod]
    public void ExistePorNickname_NicknameInexistente_DevuelveFalse()
    {
        _baseDeDatos.Contexto.Usuarios.Add(CrearUsuario(Email, Rol.Jugador, "hash-jugador"));
        _baseDeDatos.Contexto.SaveChanges();

        var existe = _repositorio.ExistePorNickname("otronickname");

        Assert.IsFalse(existe);
    }

    [TestMethod]
    public void Agregar_UsuarioNuevo_QuedaGuardadoConSuId()
    {
        var fechaNacimiento = new DateOnly(1998, 3, 10);
        var usuario = new Usuario("Ana", "Pérez", "ana@farmeort.com", "anaperez", fechaNacimiento, Rol.Jugador, "hash-ana", new DateOnly(2026, 9, 30));

        _repositorio.Agregar(usuario);
        _baseDeDatos.Contexto.ChangeTracker.Clear();

        Assert.IsGreaterThan(1, usuario.Id);
        var guardado = _baseDeDatos.Contexto.Usuarios.SingleOrDefault(u => u.Id == usuario.Id);
        Assert.IsNotNull(guardado);
        Assert.AreEqual("Ana", guardado.Nombre);
        Assert.AreEqual("Pérez", guardado.Apellido);
        Assert.AreEqual("ana@farmeort.com", guardado.Email);
        Assert.AreEqual("anaperez", guardado.Nickname);
        Assert.AreEqual(fechaNacimiento, guardado.FechaNacimiento);
        Assert.AreEqual(Rol.Jugador, guardado.Rol);
        Assert.AreEqual("hash-ana", guardado.ContrasenaHash);
        Assert.AreEqual(1200, guardado.Elo);
    }

    private static Usuario CrearUsuario(string email, Rol rol, string contrasenaHash)
    {
        var nickname = email.Split('@')[0];
        return new Usuario("Nombre", "Apellido", email, nickname, new DateOnly(2000, 1, 15), rol, contrasenaHash, new DateOnly(2026, 9, 30));
    }
}
