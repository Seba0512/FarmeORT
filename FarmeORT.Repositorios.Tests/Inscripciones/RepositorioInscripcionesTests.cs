using FarmeORT.Dominio.Inscripciones;
using FarmeORT.Dominio.Torneos;
using FarmeORT.Dominio.Usuarios;
using FarmeORT.Repositorios.Inscripciones;
using Microsoft.EntityFrameworkCore;

namespace FarmeORT.Repositorios.Tests.Inscripciones;

[TestClass]
public sealed class RepositorioInscripcionesTests
{
    private static readonly DateOnly FechaActual = new(2026, 10, 1);
    private static readonly DateTime Ahora = new(2026, 10, 1, 12, 30, 0);

    private BaseDeDatosEnMemoria _baseDeDatos = null!;
    private RepositorioInscripciones _repositorio = null!;
    private int _organizadorId;

    [TestInitialize]
    public void Inicializar()
    {
        _baseDeDatos = new BaseDeDatosEnMemoria();
        _repositorio = new RepositorioInscripciones(_baseDeDatos.Contexto);
        _organizadorId = GuardarUsuario("laurag", Rol.Organizador);
    }

    [TestCleanup]
    public void Limpiar()
    {
        _baseDeDatos.Dispose();
    }

    [TestMethod]
    public void Agregar_InscripcionNueva_QuedaGuardadaConSuId()
    {
        var torneoId = GuardarTorneo("COPAAURA");
        var jugadorId = GuardarUsuario("anaperez", Rol.Jugador);
        var inscripcion = CrearInscripcion(torneoId, jugadorId);

        _repositorio.Agregar(inscripcion);
        _baseDeDatos.Contexto.ChangeTracker.Clear();

        Assert.IsGreaterThan(0, inscripcion.Id);
        var guardada = _baseDeDatos.Contexto.Inscripciones.Single(i => i.Id == inscripcion.Id);
        Assert.AreEqual(torneoId, guardada.TorneoId);
        Assert.AreEqual(jugadorId, guardada.JugadorId);
        Assert.AreEqual(EstadoInscripcion.Aceptada, guardada.Estado);
        Assert.AreEqual(Ahora, guardada.Fecha);
    }

    [TestMethod]
    public void ObtenerPorId_IdExistente_DevuelveLaInscripcionConSuTorneoYSuJugador()
    {
        var torneoId = GuardarTorneo("COPAAURA");
        var id = GuardarInscripcion(torneoId, GuardarUsuario("anaperez", Rol.Jugador));

        var resultado = _repositorio.ObtenerPorId(id);

        Assert.IsNotNull(resultado);
        Assert.AreEqual("COPAAURA", resultado.Torneo.Codigo);
        Assert.AreEqual(_organizadorId, resultado.Torneo.OrganizadorId);
        Assert.AreEqual("anaperez", resultado.Jugador.Nickname);
        Assert.IsNull(_repositorio.ObtenerPorId(99));
    }

    [TestMethod]
    public void ExisteAceptada_SoloCuentaLaAceptadaDelJugadorEnEseTorneo()
    {
        var torneoId = GuardarTorneo("COPAAURA");
        var otroTorneoId = GuardarTorneo("COPAFUEGO");
        var aceptado = GuardarUsuario("anaperez", Rol.Jugador);
        var cancelado = GuardarUsuario("brunolopez", Rol.Jugador);
        GuardarInscripcion(torneoId, aceptado);
        GuardarInscripcion(torneoId, cancelado, i => i.Cancelar());

        Assert.IsTrue(_repositorio.ExisteAceptada(torneoId, aceptado));
        Assert.IsFalse(_repositorio.ExisteAceptada(torneoId, cancelado));
        Assert.IsFalse(_repositorio.ExisteAceptada(otroTorneoId, aceptado));
    }

    [TestMethod]
    public void ContarAceptadas_TorneoConInscripcionesEnVariosEstados_CuentaSoloLasAceptadas()
    {
        var torneoId = GuardarTorneo("COPAAURA");
        var otroTorneoId = GuardarTorneo("COPAFUEGO");
        GuardarInscripcion(torneoId, GuardarUsuario("anaperez", Rol.Jugador));
        GuardarInscripcion(torneoId, GuardarUsuario("brunolopez", Rol.Jugador));
        GuardarInscripcion(torneoId, GuardarUsuario("carlagomez", Rol.Jugador), i => i.Cancelar());
        GuardarInscripcion(torneoId, GuardarUsuario("diegoruiz", Rol.Jugador), i => i.Eliminar());
        GuardarInscripcion(otroTorneoId, GuardarUsuario("evasosa", Rol.Jugador));

        var aceptadas = _repositorio.ContarAceptadas(torneoId);

        Assert.AreEqual(2, aceptadas);
    }

    // Guarda una inscripción directamente con el contexto, sin usar métodos del repositorio (FIRST).
    private int GuardarInscripcion(int torneoId, int jugadorId, Action<Inscripcion>? cambiar = null)
    {
        var inscripcion = CrearInscripcion(torneoId, jugadorId);
        cambiar?.Invoke(inscripcion);
        _baseDeDatos.Contexto.Inscripciones.Add(inscripcion);
        _baseDeDatos.Contexto.SaveChanges();
        _baseDeDatos.Contexto.ChangeTracker.Clear();
        return inscripcion.Id;
    }

    // El torneo y el jugador se toman del contexto, como en la API: el servicio los obtiene con el mismo contexto.
    private Inscripcion CrearInscripcion(int torneoId, int jugadorId)
    {
        var torneo = _baseDeDatos.Contexto.Torneos.Include(t => t.SkillsObligatorias).Single(t => t.Id == torneoId);
        var jugador = _baseDeDatos.Contexto.Usuarios.Include(u => u.Repertorio).Single(u => u.Id == jugadorId);
        return new Inscripcion(torneo, jugador, Ahora);
    }

    private int GuardarTorneo(string codigo)
    {
        var organizador = _baseDeDatos.Contexto.Usuarios.Single(u => u.Id == _organizadorId);
        var torneo = new Torneo(codigo, "Copa Aura", "Torneo eliminatorio de batallas de aura.", new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 30), 8, organizador, FechaActual);
        _baseDeDatos.Contexto.Torneos.Add(torneo);
        _baseDeDatos.Contexto.SaveChanges();
        _baseDeDatos.Contexto.ChangeTracker.Clear();
        return torneo.Id;
    }

    private int GuardarUsuario(string nickname, Rol rol)
    {
        var usuario = new Usuario("Nombre", "Apellido", nickname + "@farmeort.com", nickname, new DateOnly(2000, 1, 15), rol, "hash", FechaActual);
        _baseDeDatos.Contexto.Usuarios.Add(usuario);
        _baseDeDatos.Contexto.SaveChanges();
        _baseDeDatos.Contexto.ChangeTracker.Clear();
        return usuario.Id;
    }
}
