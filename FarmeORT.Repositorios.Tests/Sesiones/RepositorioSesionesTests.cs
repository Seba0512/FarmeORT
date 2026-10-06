using FarmeORT.Dominio.Sesiones;
using FarmeORT.Dominio.Usuarios;
using FarmeORT.Excepciones;
using FarmeORT.Repositorios.Sesiones;
using FarmeORT.Repositorios.Usuarios;
using Microsoft.EntityFrameworkCore;

namespace FarmeORT.Repositorios.Tests.Sesiones;

[TestClass]
public sealed class RepositorioSesionesTests
{
    private static readonly Guid Token = Guid.Parse("3f2b8c1e-5d4a-4e7b-9c6f-1a2b3c4d5e6f");

    private BaseDeDatosEnMemoria _baseDeDatos = null!;
    private RepositorioSesiones _repositorio = null!;

    [TestInitialize]
    public void Inicializar()
    {
        _baseDeDatos = new BaseDeDatosEnMemoria();
        _repositorio = new RepositorioSesiones(_baseDeDatos.Contexto);
    }

    [TestCleanup]
    public void Limpiar()
    {
        _baseDeDatos.Dispose();
    }

    [TestMethod]
    public void Agregar_SesionNueva_QuedaGuardadaConSuUsuario()
    {
        var usuario = CrearUsuario("jugador@farmeort.com", Rol.Jugador, "hash-jugador");
        _baseDeDatos.Contexto.Usuarios.Add(usuario);
        _baseDeDatos.Contexto.SaveChanges();

        _repositorio.Agregar(new Sesion(Token, usuario));
        _baseDeDatos.Contexto.ChangeTracker.Clear();

        var guardada = _baseDeDatos.Contexto.Sesiones.SingleOrDefault(s => s.Token == Token);
        Assert.IsNotNull(guardada);
        Assert.AreEqual(usuario.Id, guardada.UsuarioId);
    }

    [TestMethod]
    public void Eliminar_TokenExistente_BorraSoloEsaSesion()
    {
        var otroToken = Guid.Parse("9a8b7c6d-5e4f-4a3b-8c2d-1e0f9a8b7c6d");
        var usuario = CrearUsuario("jugador@farmeort.com", Rol.Jugador, "hash-jugador");
        _baseDeDatos.Contexto.Usuarios.Add(usuario);
        _baseDeDatos.Contexto.Sesiones.AddRange(new Sesion(Token, usuario), new Sesion(otroToken, usuario));
        _baseDeDatos.Contexto.SaveChanges();
        _baseDeDatos.Contexto.ChangeTracker.Clear();

        _repositorio.Eliminar(Token);

        Assert.IsFalse(_baseDeDatos.Contexto.Sesiones.Any(s => s.Token == Token));
        Assert.IsTrue(_baseDeDatos.Contexto.Sesiones.Any(s => s.Token == otroToken));
    }

    [TestMethod]
    public void Eliminar_TokenInexistente_NoBorraNingunaSesion()
    {
        var otroToken = Guid.Parse("9a8b7c6d-5e4f-4a3b-8c2d-1e0f9a8b7c6d");
        var usuario = CrearUsuario("jugador@farmeort.com", Rol.Jugador, "hash-jugador");
        _baseDeDatos.Contexto.Usuarios.Add(usuario);
        _baseDeDatos.Contexto.Sesiones.Add(new Sesion(otroToken, usuario));
        _baseDeDatos.Contexto.SaveChanges();

        _repositorio.Eliminar(Token);

        Assert.AreEqual(1, _baseDeDatos.Contexto.Sesiones.Count());
    }

    [TestMethod]
    public void ObtenerPorToken_TokenExistente_DevuelveSesionConSuUsuario()
    {
        var usuario = CrearUsuario("organizador@farmeort.com", Rol.Organizador, "hash-organizador");
        _baseDeDatos.Contexto.Usuarios.Add(usuario);
        _baseDeDatos.Contexto.Sesiones.Add(new Sesion(Token, usuario));
        _baseDeDatos.Contexto.SaveChanges();
        _baseDeDatos.Contexto.ChangeTracker.Clear();

        var resultado = _repositorio.ObtenerPorToken(Token);

        Assert.IsNotNull(resultado);
        Assert.AreEqual(Token, resultado.Token);
        Assert.IsNotNull(resultado.Usuario);
        Assert.AreEqual(usuario.Id, resultado.Usuario.Id);
        Assert.AreEqual(Rol.Organizador, resultado.Usuario.Rol);
    }

    [TestMethod]
    public void ObtenerPorToken_TokenInexistente_DevuelveNull()
    {
        var resultado = _repositorio.ObtenerPorToken(Token);

        Assert.IsNull(resultado);
    }

    [TestMethod]
    [DataRow(Rol.Jugador)]
    [DataRow(Rol.Organizador)]
    [DataRow(Rol.Administrador)]
    public void Agregar_BajaConfirmadaTrasLeerUsuario_RechazaSesionSinReactivarUsuario(Rol rol)
    {
        var usuario = CrearUsuario("usuario@farmeort.com", rol, "hash");
        _baseDeDatos.Contexto.Usuarios.Add(usuario);
        _baseDeDatos.Contexto.SaveChanges();
        using var login = _baseDeDatos.CrearContexto();
        using var baja = _baseDeDatos.CrearContexto();
        var leido = login.Usuarios.Single(u => u.Id == usuario.Id);
        var eliminado = baja.Usuarios.Single(u => u.Id == usuario.Id);
        eliminado.Eliminar();
        new RepositorioUsuarios(baja).Eliminar(eliminado);

        Assert.ThrowsExactly<CredencialesInvalidasException>(() => new RepositorioSesiones(login).Agregar(new Sesion(Token, leido)));

        using var verificacion = _baseDeDatos.CrearContexto();
        Assert.IsTrue(verificacion.Usuarios.Single(u => u.Id == usuario.Id).Eliminado);
        Assert.AreEqual(1200, verificacion.Usuarios.Single(u => u.Id == usuario.Id).Elo);
        Assert.IsFalse(verificacion.Sesiones.Any(s => s.Token == Token));
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void ObtenerPorToken_BajaConcurrenteConSesionResidual_NoDevuelveSesion(bool cargarAntes)
    {
        var usuario = CrearUsuario("residual@farmeort.com", Rol.Organizador, "hash");
        _baseDeDatos.Contexto.Sesiones.Add(new Sesion(Token, usuario));
        _baseDeDatos.Contexto.SaveChanges();
        using var lectura = _baseDeDatos.CrearContexto();
        if(cargarAntes)
        {
            lectura.Sesiones.Include(s => s.Usuario).Load();
        }

        using var baja = _baseDeDatos.CrearContexto();
        var eliminado = baja.Usuarios.Single(u => u.Id == usuario.Id);
        eliminado.Eliminar();
        baja.SaveChanges();

        var resultado = new RepositorioSesiones(lectura).ObtenerPorToken(Token);

        Assert.IsNull(resultado);
    }

    [TestMethod]
    public void Agregar_LoginConfirmadoTrasLeerBaja_LaBajaRevocaLaSesion()
    {
        var usuario = CrearUsuario("primero@farmeort.com", Rol.Jugador, "hash");
        usuario.ActualizarElo(1300);
        _baseDeDatos.Contexto.Usuarios.Add(usuario);
        _baseDeDatos.Contexto.SaveChanges();
        using var baja = _baseDeDatos.CrearContexto();
        var eliminado = baja.Usuarios.Single(u => u.Id == usuario.Id);
        eliminado.Eliminar();
        using var login = _baseDeDatos.CrearContexto();
        var activo = login.Usuarios.Single(u => u.Id == usuario.Id);
        new RepositorioSesiones(login).Agregar(new Sesion(Token, activo));

        new RepositorioUsuarios(baja).Eliminar(eliminado);

        using var verificacion = _baseDeDatos.CrearContexto();
        Assert.IsFalse(verificacion.Sesiones.Any(s => s.UsuarioId == usuario.Id));
        var guardado = verificacion.Usuarios.Single(u => u.Id == usuario.Id);
        Assert.IsTrue(guardado.Eliminado);
        Assert.AreEqual(1300, guardado.Elo);
        Assert.AreEqual(usuario.Nickname, guardado.Nickname);
    }

    [TestMethod]
    public void Agregar_TokenDuplicado_RevierteAltaYLiberaTransaccion()
    {
        var usuario = CrearUsuario("rollback@farmeort.com", Rol.Jugador, "hash");
        _baseDeDatos.Contexto.Sesiones.Add(new Sesion(Token, usuario));
        _baseDeDatos.Contexto.SaveChanges();
        using var login = _baseDeDatos.CrearContexto();
        var leido = login.Usuarios.Single(u => u.Id == usuario.Id);

        Assert.ThrowsExactly<DbUpdateException>(() => new RepositorioSesiones(login).Agregar(new Sesion(Token, leido)));

        Assert.IsNull(login.Database.CurrentTransaction);
        using var verificacion = _baseDeDatos.CrearContexto();
        Assert.AreEqual(1, verificacion.Sesiones.Count(s => s.UsuarioId == usuario.Id));
        Assert.IsFalse(verificacion.Usuarios.Single(u => u.Id == usuario.Id).Eliminado);
    }

    private static Usuario CrearUsuario(string email, Rol rol, string contrasenaHash)
    {
        var nickname = email.Split('@')[0];
        return new Usuario("Nombre", "Apellido", email, nickname, new DateOnly(2000, 1, 15), rol, contrasenaHash, new DateOnly(2026, 9, 30));
    }
}
