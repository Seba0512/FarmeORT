using FarmeORT.Dominio.Inscripciones;
using FarmeORT.Dominio.Skills;
using FarmeORT.Dominio.Torneos;
using FarmeORT.Dominio.Usuarios;
using FarmeORT.Excepciones;
using FarmeORT.IRepositorios.Inscripciones;
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
    [DataRow(EstadoInscripcion.Aceptada)]
    [DataRow(EstadoInscripcion.Eliminada)]
    [DataRow(EstadoInscripcion.Cancelada)]
    public void ExisteDelJugador_InscripcionHistorica_FiltraTorneoYJugadorSinExcluirEstados(EstadoInscripcion estado)
    {
        var torneoId = GuardarTorneo("COPAAURA");
        var otroTorneoId = GuardarTorneo("COPAFUEGO");
        var jugadorId = GuardarUsuario("anaperez", Rol.Jugador);
        var otroJugadorId = GuardarUsuario("brunolopez", Rol.Jugador);
        GuardarInscripcion(torneoId, jugadorId, i =>
        {
            if(estado == EstadoInscripcion.Eliminada)
            {
                i.Eliminar();
            }
            else if(estado == EstadoInscripcion.Cancelada)
            {
                i.Cancelar();
            }
        });
        _baseDeDatos.Contexto.ChangeTracker.Clear();

        Assert.IsTrue(_repositorio.ExisteDelJugador(torneoId, jugadorId));
        Assert.IsFalse(_repositorio.ExisteDelJugador(otroTorneoId, jugadorId));
        Assert.IsFalse(_repositorio.ExisteDelJugador(torneoId, otroJugadorId));
        Assert.IsFalse(_repositorio.ExisteDelJugador(99, 99));
        Assert.AreEqual(0, _baseDeDatos.Contexto.ChangeTracker.Entries().Count());
    }

    [TestMethod]
    public void ObtenerAceptada_FiltraTorneoJugadorYEstadoYDevuelveEntidadRastreada()
    {
        var torneoId = GuardarTorneo("COPAAURA");
        var otroTorneoId = GuardarTorneo("COPAFUEGO");
        var jugadorId = GuardarUsuario("anaperez", Rol.Jugador);
        var eliminadoId = GuardarUsuario("brunolopez", Rol.Jugador);
        GuardarInscripcion(torneoId, jugadorId, i => i.Cancelar());
        var aceptadaId = GuardarInscripcion(torneoId, jugadorId);
        GuardarInscripcion(otroTorneoId, jugadorId);
        GuardarInscripcion(torneoId, eliminadoId, i => i.Eliminar());

        var resultado = _repositorio.ObtenerAceptada(torneoId, jugadorId);

        Assert.IsNotNull(resultado);
        Assert.AreEqual(aceptadaId, resultado.Id);
        Assert.AreEqual(EntityState.Unchanged, _baseDeDatos.Contexto.Entry(resultado).State);
        Assert.IsNull(_repositorio.ObtenerAceptada(torneoId, eliminadoId));
        Assert.IsNull(_repositorio.ObtenerAceptada(99, jugadorId));
        Assert.IsNull(_repositorio.ObtenerAceptada(torneoId, 99));
    }

    [TestMethod]
    public void Agregar_DosPedidosDelMismoJugadorLeenAntesDeGuardar_SoloUnoQuedaAceptado()
    {
        var torneoId = GuardarTorneo("COPAAURA");
        var jugadorId = GuardarUsuario("anaperez", Rol.Jugador);
        using var primero = _baseDeDatos.CrearContexto();
        using var segundo = _baseDeDatos.CrearContexto();
        var primera = new Inscripcion(primero.Torneos.Single(t => t.Id == torneoId), primero.Usuarios.Single(u => u.Id == jugadorId), Ahora);
        var segunda = new Inscripcion(segundo.Torneos.Single(t => t.Id == torneoId), segundo.Usuarios.Single(u => u.Id == jugadorId), Ahora);
        var primerRepositorio = new RepositorioInscripciones(primero);
        var segundoRepositorio = new RepositorioInscripciones(segundo);
        Assert.IsFalse(primerRepositorio.ExisteAceptada(torneoId, jugadorId));
        Assert.IsFalse(segundoRepositorio.ExisteAceptada(torneoId, jugadorId));
        Assert.AreEqual(0, primerRepositorio.ContarAceptadas(torneoId));
        Assert.AreEqual(0, segundoRepositorio.ContarAceptadas(torneoId));
        primerRepositorio.Agregar(primera);

        Assert.ThrowsExactly<InscripcionYaExisteException>(() => segundoRepositorio.Agregar(segunda));

        Assert.AreEqual(1, _baseDeDatos.Contexto.Inscripciones.Count(i => i.TorneoId == torneoId && i.Estado == EstadoInscripcion.Aceptada));
        Assert.IsEmpty(segundo.ChangeTracker.Entries().ToList());
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

    [TestMethod]
    public void ContarAceptadasPorTorneo_VariosTorneos_DevuelveLaCantidadDeCadaUnoDeLosPedidos()
    {
        var primero = GuardarTorneo("COPAAURA");
        var segundo = GuardarTorneo("COPAFUEGO");
        var noPedido = GuardarTorneo("COPAHIELO");
        GuardarInscripcion(primero, GuardarUsuario("anaperez", Rol.Jugador));
        GuardarInscripcion(primero, GuardarUsuario("brunolopez", Rol.Jugador));
        GuardarInscripcion(segundo, GuardarUsuario("carlagomez", Rol.Jugador));
        GuardarInscripcion(segundo, GuardarUsuario("diegoruiz", Rol.Jugador), i => i.Cancelar());
        GuardarInscripcion(noPedido, GuardarUsuario("evasosa", Rol.Jugador));

        var aceptadas = _repositorio.ContarAceptadasPorTorneo([primero, segundo]);

        Assert.HasCount(2, aceptadas);
        Assert.AreEqual(2, aceptadas[primero]);
        Assert.AreEqual(1, aceptadas[segundo]);
    }

    [TestMethod]
    public void ListarAceptadasDelTorneo_TraeSoloLasAceptadasConElRepertorioDeCadaJugador()
    {
        var torneoId = GuardarTorneo("COPAAURA");
        var skill = new Skill("SIXSEVEN", "Six-Seven", "Gesto alternando las manos hacia arriba y abajo.", CategoriaSkill.Gestual, 2);
        _baseDeDatos.Contexto.Skills.Add(skill);
        _baseDeDatos.Contexto.SaveChanges();
        var conSkill = _baseDeDatos.Contexto.Usuarios.Include(u => u.Repertorio).Single(u => u.Id == GuardarUsuario("anaperez", Rol.Jugador));
        conSkill.DeclararSkill(skill, 3, FechaActual);
        _baseDeDatos.Contexto.SaveChanges();
        _baseDeDatos.Contexto.ChangeTracker.Clear();
        GuardarInscripcion(torneoId, conSkill.Id);
        GuardarInscripcion(torneoId, GuardarUsuario("brunolopez", Rol.Jugador), i => i.Cancelar());

        var aceptadas = _repositorio.ListarAceptadasDelTorneo(torneoId);

        var inscripcion = aceptadas.Single();
        Assert.AreEqual("anaperez", inscripcion.Jugador.Nickname);
        Assert.AreEqual(skill.Id, inscripcion.Jugador.Repertorio.Single().SkillId);
    }

    [TestMethod]
    public void ListarAceptadasDelJugador_TraeSoloSusAceptadasConLasObligatoriasDeCadaTorneo()
    {
        var torneoId = GuardarTorneo("COPAAURA");
        var otroTorneoId = GuardarTorneo("COPAFUEGO");
        var jugadorId = GuardarUsuario("anaperez", Rol.Jugador);
        GuardarInscripcion(torneoId, jugadorId);
        GuardarInscripcion(otroTorneoId, jugadorId, i => i.Eliminar());
        GuardarInscripcion(torneoId, GuardarUsuario("brunolopez", Rol.Jugador));
        var skill = new Skill("SIXSEVEN", "Six-Seven", "Gesto alternando las manos hacia arriba y abajo.", CategoriaSkill.Gestual, 2);
        _baseDeDatos.Contexto.Skills.Add(skill);
        _baseDeDatos.Contexto.Torneos.Include(t => t.SkillsObligatorias).Single(t => t.Id == torneoId).AsociarSkillObligatoria(skill);
        _baseDeDatos.Contexto.SaveChanges();
        _baseDeDatos.Contexto.ChangeTracker.Clear();

        var aceptadas = _repositorio.ListarAceptadasDelJugador(jugadorId);

        var inscripcion = aceptadas.Single();
        Assert.AreEqual(torneoId, inscripcion.TorneoId);
        Assert.AreEqual("SIXSEVEN", inscripcion.Torneo.SkillsObligatorias.Single().Codigo);
    }

    [TestMethod]
    public void Listar_SinFiltrosConPaginacion_DevuelveLaPaginaEnOrdenDeIdConTorneoYJugador()
    {
        var torneoId = GuardarTorneo("COPAAURA");
        var primera = GuardarInscripcion(torneoId, GuardarUsuario("anaperez", Rol.Jugador));
        var segunda = GuardarInscripcion(torneoId, GuardarUsuario("brunolopez", Rol.Jugador));
        GuardarInscripcion(torneoId, GuardarUsuario("carlagomez", Rol.Jugador));

        var inscripciones = _repositorio.Listar(Filtro(limite: 2));

        CollectionAssert.AreEqual(new[] { primera, segunda }, inscripciones.Select(i => i.Id).ToArray());
        Assert.AreEqual("COPAAURA", inscripciones[0].Torneo.Codigo);
        Assert.AreEqual("anaperez", inscripciones[0].Jugador.Nickname);
        Assert.AreEqual(segunda, _repositorio.Listar(Filtro(limite: 1, desplazamiento: 1)).Single().Id);
    }

    [TestMethod]
    public void Listar_FiltrosPorTorneoEstadoYFechas_DevuelveLasQueCumplenTodos()
    {
        var torneoId = GuardarTorneo("COPAAURA");
        var otroTorneoId = GuardarTorneo("COPAFUEGO");
        var buscada = GuardarInscripcion(torneoId, GuardarUsuario("anaperez", Rol.Jugador));
        GuardarInscripcion(torneoId, GuardarUsuario("brunolopez", Rol.Jugador), i => i.Cancelar());
        GuardarInscripcion(otroTorneoId, GuardarUsuario("carlagomez", Rol.Jugador));
        var dia = DateOnly.FromDateTime(Ahora);

        var inscripciones = _repositorio.Listar(Filtro(torneoId, EstadoInscripcion.Aceptada, dia, dia));
        var fueraDeRango = _repositorio.Listar(Filtro(fechaDesde: dia.AddDays(1)));

        CollectionAssert.AreEqual(new[] { buscada }, inscripciones.Select(i => i.Id).ToArray());
        Assert.IsEmpty(fueraDeRango);
    }

    [TestMethod]
    public void Listar_RestringidoAUnJugadorOAUnOrganizador_DevuelveSoloLasQueLeCorresponden()
    {
        var torneoPropio = GuardarTorneo("COPAAURA");
        var otroOrganizadorId = GuardarUsuario("pedroo", Rol.Organizador);
        var torneoAjeno = GuardarTorneo("COPAFUEGO", otroOrganizadorId);
        var jugadorId = GuardarUsuario("anaperez", Rol.Jugador);
        var delJugador = GuardarInscripcion(torneoPropio, jugadorId);
        var deOtroJugador = GuardarInscripcion(torneoPropio, GuardarUsuario("brunolopez", Rol.Jugador));
        var delJugadorEnTorneoAjeno = GuardarInscripcion(torneoAjeno, jugadorId);

        var delJugadorListadas = _repositorio.Listar(Filtro(jugadorId: jugadorId));
        var delOrganizadorListadas = _repositorio.Listar(Filtro(organizadorId: _organizadorId));

        CollectionAssert.AreEqual(new[] { delJugador, delJugadorEnTorneoAjeno }, delJugadorListadas.Select(i => i.Id).ToArray());
        CollectionAssert.AreEqual(new[] { delJugador, deOtroJugador }, delOrganizadorListadas.Select(i => i.Id).ToArray());
    }

    [TestMethod]
    public void Actualizar_InscripcionCancelada_GuardaElEstado()
    {
        var id = GuardarInscripcion(GuardarTorneo("COPAAURA"), GuardarUsuario("anaperez", Rol.Jugador));
        var inscripcion = _baseDeDatos.Contexto.Inscripciones.Include(i => i.Torneo).Include(i => i.Jugador).Single(i => i.Id == id);
        inscripcion.Cancelar();

        _repositorio.Actualizar(inscripcion);
        _baseDeDatos.Contexto.ChangeTracker.Clear();

        Assert.AreEqual(EstadoInscripcion.Cancelada, _baseDeDatos.Contexto.Inscripciones.Single(i => i.Id == id).Estado);
    }

    // Arma el filtro del repositorio indicando solo lo que cada test necesita; el resto no filtra.
    private static FiltroInscripciones Filtro(
        int? torneoId = null,
        EstadoInscripcion? estado = null,
        DateOnly? fechaDesde = null,
        DateOnly? fechaHasta = null,
        int? jugadorId = null,
        int? organizadorId = null,
        int limite = 20,
        int desplazamiento = 0)
    {
        return new FiltroInscripciones(torneoId, estado, fechaDesde, fechaHasta, jugadorId, organizadorId, limite, desplazamiento);
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

    private int GuardarTorneo(string codigo, int? organizadorId = null)
    {
        var organizador = _baseDeDatos.Contexto.Usuarios.Single(u => u.Id == (organizadorId ?? _organizadorId));
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
