using FarmeORT.Dominio.Usuarios;
using FarmeORT.IRepositorios.Usuarios;
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
    public void Listar_SinFiltros_DevuelveTodosOrdenadosPorId()
    {
        _baseDeDatos.Contexto.Usuarios.AddRange(
            CrearUsuario("laura@farmeort.com", Rol.Organizador, "hash-laura"),
            CrearUsuario("carlos@farmeort.com", Rol.Jugador, "hash-carlos"));
        _baseDeDatos.Contexto.SaveChanges();
        _baseDeDatos.Contexto.ChangeTracker.Clear();

        var resultado = _repositorio.Listar(new FiltroUsuarios(null, null, null, 20, 0));

        string[] emailsEsperados = ["admin@farmeort.com", "laura@farmeort.com", "carlos@farmeort.com"];
        CollectionAssert.AreEqual(emailsEsperados, resultado.Select(u => u.Email).ToArray());
    }

    [TestMethod]
    public void Listar_FiltroPorNombre_DevuelveLosQueLoContienenSinDistinguirMayusculas()
    {
        _baseDeDatos.Contexto.Usuarios.AddRange(
            CrearUsuario("carlos@farmeort.com", Rol.Jugador, "hash-carlos", nombre: "Carlos"),
            CrearUsuario("laura@farmeort.com", Rol.Organizador, "hash-laura", nombre: "Laura"),
            CrearUsuario("oscar@farmeort.com", Rol.Jugador, "hash-oscar", nombre: "Oscar"));
        _baseDeDatos.Contexto.SaveChanges();
        _baseDeDatos.Contexto.ChangeTracker.Clear();

        var resultado = _repositorio.Listar(new FiltroUsuarios("CAR", null, null, 20, 0));

        string[] nombresEsperados = ["Carlos", "Oscar"];
        CollectionAssert.AreEqual(nombresEsperados, resultado.Select(u => u.Nombre).ToArray());
    }

    [TestMethod]
    public void Listar_FiltroPorApellido_DevuelveLosQueLoContienenSinDistinguirMayusculas()
    {
        _baseDeDatos.Contexto.Usuarios.AddRange(
            CrearUsuario("carlos@farmeort.com", Rol.Jugador, "hash-carlos", apellido: "Suarez"),
            CrearUsuario("laura@farmeort.com", Rol.Organizador, "hash-laura", apellido: "Gomez"),
            CrearUsuario("oscar@farmeort.com", Rol.Jugador, "hash-oscar", apellido: "Juarez"));
        _baseDeDatos.Contexto.SaveChanges();
        _baseDeDatos.Contexto.ChangeTracker.Clear();

        var resultado = _repositorio.Listar(new FiltroUsuarios(null, "UAREZ", null, 20, 0));

        string[] apellidosEsperados = ["Suarez", "Juarez"];
        CollectionAssert.AreEqual(apellidosEsperados, resultado.Select(u => u.Apellido).ToArray());
    }

    [TestMethod]
    public void Listar_FiltroPorNickname_DevuelveLosQueLoContienenSinDistinguirMayusculas()
    {
        _baseDeDatos.Contexto.Usuarios.AddRange(
            CrearUsuario("carlossuarez@farmeort.com", Rol.Jugador, "hash-carlos"),
            CrearUsuario("lauragomez@farmeort.com", Rol.Organizador, "hash-laura"),
            CrearUsuario("oscarsuarez@farmeort.com", Rol.Jugador, "hash-oscar"));
        _baseDeDatos.Contexto.SaveChanges();
        _baseDeDatos.Contexto.ChangeTracker.Clear();

        var resultado = _repositorio.Listar(new FiltroUsuarios(null, null, "SUAREZ", 20, 0));

        string[] nicknamesEsperados = ["carlossuarez", "oscarsuarez"];
        CollectionAssert.AreEqual(nicknamesEsperados, resultado.Select(u => u.Nickname).ToArray());
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

    private static Usuario CrearUsuario(
        string email,
        Rol rol,
        string contrasenaHash,
        string nombre = "Nombre",
        string apellido = "Apellido")
    {
        var nickname = email.Split('@')[0];
        return new Usuario(nombre, apellido, email, nickname, new DateOnly(2000, 1, 15), rol, contrasenaHash, new DateOnly(2026, 9, 30));
    }
}
