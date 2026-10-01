using FarmeORT.Dominio.Sesiones;
using FarmeORT.Dominio.Usuarios;
using FarmeORT.Excepciones;
using FarmeORT.IRepositorios.Usuarios;
using FarmeORT.Repositorios.Usuarios;
using Microsoft.EntityFrameworkCore;

namespace FarmeORT.Repositorios.Tests.Usuarios;

[TestClass]
public sealed class RepositorioUsuariosTests
{
    private const string Email = "jugador@farmeort.com";

    private static readonly DateOnly FechaActual = new(2026, 9, 30);

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
    public void Listar_VariosFiltros_DevuelveLosQueCumplenTodos()
    {
        _baseDeDatos.Contexto.Usuarios.AddRange(
            CrearUsuario("carlossuarez@farmeort.com", Rol.Jugador, "hash-carlos", nombre: "Carlos", apellido: "Suarez"),
            CrearUsuario("carlosgomez@farmeort.com", Rol.Jugador, "hash-carlos2", nombre: "Carlos", apellido: "Gomez"),
            CrearUsuario("laurasuarez@farmeort.com", Rol.Organizador, "hash-laura", nombre: "Laura", apellido: "Suarez"));
        _baseDeDatos.Contexto.SaveChanges();
        _baseDeDatos.Contexto.ChangeTracker.Clear();

        var resultado = _repositorio.Listar(new FiltroUsuarios("carlos", "suarez", null, 20, 0));

        Assert.HasCount(1, resultado);
        Assert.AreEqual("carlossuarez", resultado[0].Nickname);
    }

    [TestMethod]
    public void Listar_ConLimiteYDesplazamiento_DevuelveLaPaginaEnOrdenDeId()
    {
        _baseDeDatos.Contexto.Usuarios.AddRange(
            CrearUsuario("usuario2@farmeort.com", Rol.Jugador, "hash-2"),
            CrearUsuario("usuario3@farmeort.com", Rol.Jugador, "hash-3"),
            CrearUsuario("usuario4@farmeort.com", Rol.Jugador, "hash-4"),
            CrearUsuario("usuario5@farmeort.com", Rol.Jugador, "hash-5"));
        _baseDeDatos.Contexto.SaveChanges();
        _baseDeDatos.Contexto.ChangeTracker.Clear();

        var resultado = _repositorio.Listar(new FiltroUsuarios(null, null, null, 2, 2));

        string[] emailsEsperados = ["usuario3@farmeort.com", "usuario4@farmeort.com"];
        CollectionAssert.AreEqual(emailsEsperados, resultado.Select(u => u.Email).ToArray());
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

    [TestMethod]
    public void Agregar_EmailOcupadoDespuesDeConsultar_LanzaEmailYaExisteSinDuplicar()
    {
        Assert.IsNull(_repositorio.ObtenerPorEmail(Email));
        _baseDeDatos.Contexto.Usuarios.Add(CrearUsuario(Email, Rol.Jugador, "hash-existente"));
        _baseDeDatos.Contexto.SaveChanges();
        _baseDeDatos.Contexto.ChangeTracker.Clear();
        var nuevo = new Usuario("Ana", "Pérez", Email, "otronickname", new DateOnly(2000, 1, 15), Rol.Jugador, "hash-nuevo", new DateOnly(2026, 9, 30));

        Assert.ThrowsExactly<EmailYaExisteException>(() => _repositorio.Agregar(nuevo));
        Assert.AreEqual(1, _baseDeDatos.Contexto.Usuarios.Count(u => u.Email == Email));
        AssertPuedeGuardarOtraAlta();
    }

    [TestMethod]
    public void Agregar_NicknameOcupadoDespuesDeConsultar_LanzaNicknameYaExisteSinDuplicar()
    {
        Assert.IsFalse(_repositorio.ExistePorNickname("jugador"));
        _baseDeDatos.Contexto.Usuarios.Add(CrearUsuario(Email, Rol.Jugador, "hash-existente"));
        _baseDeDatos.Contexto.SaveChanges();
        _baseDeDatos.Contexto.ChangeTracker.Clear();
        var nuevo = new Usuario("Ana", "Pérez", "otro@farmeort.com", "jugador", new DateOnly(2000, 1, 15), Rol.Jugador, "hash-nuevo", new DateOnly(2026, 9, 30));

        Assert.ThrowsExactly<NicknameYaExisteException>(() => _repositorio.Agregar(nuevo));
        Assert.AreEqual(1, _baseDeDatos.Contexto.Usuarios.Count(u => u.Nickname == "jugador"));
        AssertPuedeGuardarOtraAlta();
    }

    [TestMethod]
    public void Agregar_ErrorDeBaseSinDuplicados_PropagaDbUpdateException()
    {
        var nuevo = new Usuario("Ana", "Pérez", "otro@farmeort.com", "otronickname", new DateOnly(2000, 1, 15), Rol.Jugador, "hash-nuevo", new DateOnly(2026, 9, 30))
        {
            Id = 1,
        };

        Assert.ThrowsExactly<DbUpdateException>(() => _repositorio.Agregar(nuevo));
        Assert.IsFalse(_baseDeDatos.Contexto.Usuarios.Any(u => u.Email == nuevo.Email));
        AssertPuedeGuardarOtraAlta();
    }

    [TestMethod]
    public void Actualizar_UsuarioModificado_GuardaLosCambios()
    {
        var usuario = GuardarUsuario(Email, "jugador");
        var fechaNacimiento = new DateOnly(1998, 6, 3);
        usuario.Modificar("Carla", "Gómez", "carla@farmeort.com", "carlagomez", fechaNacimiento, Rol.Organizador, "hash-nuevo", FechaActual);

        _repositorio.Actualizar(usuario);
        _baseDeDatos.Contexto.ChangeTracker.Clear();

        var guardado = _baseDeDatos.Contexto.Usuarios.Single(u => u.Id == usuario.Id);
        Assert.AreEqual("Carla", guardado.Nombre);
        Assert.AreEqual("Gómez", guardado.Apellido);
        Assert.AreEqual("carla@farmeort.com", guardado.Email);
        Assert.AreEqual("carlagomez", guardado.Nickname);
        Assert.AreEqual(fechaNacimiento, guardado.FechaNacimiento);
        Assert.AreEqual(Rol.Organizador, guardado.Rol);
        Assert.AreEqual("hash-nuevo", guardado.ContrasenaHash);
        Assert.AreEqual(1200, guardado.Elo);
    }

    [TestMethod]
    public void Actualizar_EmailOcupadoDespuesDeConsultar_LanzaEmailYaExisteSinCambiarElUsuario()
    {
        var usuario = GuardarUsuario(Email, "jugador");
        GuardarUsuario("otro@farmeort.com", "otro");
        usuario.Modificar("Ana", "Pérez", "otro@farmeort.com", "jugador", new DateOnly(2000, 1, 15), Rol.Jugador, "hash-ana", FechaActual);

        Assert.ThrowsExactly<EmailYaExisteException>(() => _repositorio.Actualizar(usuario));
        _baseDeDatos.Contexto.ChangeTracker.Clear();
        Assert.AreEqual(Email, _baseDeDatos.Contexto.Usuarios.Single(u => u.Id == usuario.Id).Email);
        AssertPuedeGuardarOtraAlta();
    }

    [TestMethod]
    public void Actualizar_NicknameOcupadoConservandoSuEmail_LanzaNicknameYaExiste()
    {
        var usuario = GuardarUsuario(Email, "jugador");
        GuardarUsuario("otro@farmeort.com", "otro");
        usuario.Modificar("Ana", "Pérez", Email, "otro", new DateOnly(2000, 1, 15), Rol.Jugador, "hash-ana", FechaActual);

        Assert.ThrowsExactly<NicknameYaExisteException>(() => _repositorio.Actualizar(usuario));
        _baseDeDatos.Contexto.ChangeTracker.Clear();
        Assert.AreEqual("jugador", _baseDeDatos.Contexto.Usuarios.Single(u => u.Id == usuario.Id).Nickname);
        AssertPuedeGuardarOtraAlta();
    }

    [TestMethod]
    public void Actualizar_ErrorDeBaseConservandoEmailYNickname_PropagaDbUpdateException()
    {
        var usuario = GuardarUsuario(Email, "jugador");
        _baseDeDatos.Contexto.Database.ExecuteSqlRaw(
            "CREATE TRIGGER RechazaActualizaciones BEFORE UPDATE ON Usuarios BEGIN SELECT RAISE(ABORT, 'error de base'); END;");
        usuario.Modificar("Carla", "Gómez", Email, "jugador", new DateOnly(2000, 1, 15), Rol.Jugador, "hash-ana", FechaActual);

        Assert.ThrowsExactly<DbUpdateException>(() => _repositorio.Actualizar(usuario));
        AssertPuedeGuardarOtraAlta();
    }

    [TestMethod]
    public void Eliminar_UsuarioConSesiones_GuardaLaBajaYCierraSoloSusSesiones()
    {
        var guardado = CrearUsuario(Email, Rol.Jugador, "hash-jugador");
        var otro = CrearUsuario("otro@farmeort.com", Rol.Jugador, "hash-otro");
        var sesionDelOtro = Guid.NewGuid();
        _baseDeDatos.Contexto.Sesiones.AddRange(
            new Sesion(Guid.NewGuid(), guardado),
            new Sesion(Guid.NewGuid(), guardado),
            new Sesion(sesionDelOtro, otro));
        _baseDeDatos.Contexto.SaveChanges();
        _baseDeDatos.Contexto.ChangeTracker.Clear();
        var usuario = _repositorio.ObtenerPorId(guardado.Id)!;
        usuario.Eliminar();

        _repositorio.Eliminar(usuario);
        _baseDeDatos.Contexto.ChangeTracker.Clear();

        Assert.IsTrue(_baseDeDatos.Contexto.Usuarios.Single(u => u.Id == usuario.Id).Eliminado);
        Assert.IsFalse(_baseDeDatos.Contexto.Sesiones.Any(s => s.UsuarioId == usuario.Id));
        Assert.IsTrue(_baseDeDatos.Contexto.Sesiones.Any(s => s.Token == sesionDelOtro));
    }

    // Deja el usuario guardado y cargado por el repositorio, como lo recibe el servicio antes de modificarlo.
    private Usuario GuardarUsuario(string email, string nickname)
    {
        var usuario = new Usuario("Ana", "Pérez", email, nickname, new DateOnly(2000, 1, 15), Rol.Jugador, "hash-ana", FechaActual);
        _baseDeDatos.Contexto.Usuarios.Add(usuario);
        _baseDeDatos.Contexto.SaveChanges();
        _baseDeDatos.Contexto.ChangeTracker.Clear();
        return _repositorio.ObtenerPorId(usuario.Id)!;
    }

    private void AssertPuedeGuardarOtraAlta()
    {
        var siguiente = CrearUsuario("siguiente@farmeort.com", Rol.Jugador, "hash-siguiente");

        _repositorio.Agregar(siguiente);

        Assert.IsTrue(_baseDeDatos.Contexto.Usuarios.Any(u => u.Id == siguiente.Id && u.Email == siguiente.Email));
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
