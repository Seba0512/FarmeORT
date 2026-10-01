using FarmeORT.Dominio.Torneos;
using FarmeORT.Dominio.Usuarios;
using FarmeORT.Excepciones;
using FarmeORT.IRepositorios.Torneos;
using FarmeORT.Repositorios.Torneos;
using Microsoft.EntityFrameworkCore;

namespace FarmeORT.Repositorios.Tests.Torneos;

[TestClass]
public sealed class RepositorioTorneosTests
{
    private const string Descripcion = "Torneo eliminatorio de batallas de aura.";

    private static readonly DateOnly FechaActual = new(2026, 10, 1);
    private static readonly DateOnly FechaInicio = new(2026, 11, 1);
    private static readonly DateOnly FechaFin = new(2026, 11, 30);

    private BaseDeDatosEnMemoria _baseDeDatos = null!;
    private RepositorioTorneos _repositorio = null!;
    private Usuario _organizador = null!;

    [TestInitialize]
    public void Inicializar()
    {
        _baseDeDatos = new BaseDeDatosEnMemoria();
        _repositorio = new RepositorioTorneos(_baseDeDatos.Contexto);
        _organizador = GuardarOrganizador();
    }

    [TestCleanup]
    public void Limpiar()
    {
        _baseDeDatos.Dispose();
    }

    [TestMethod]
    public void Agregar_TorneoNuevo_QuedaGuardadoConSuIdYSuOrganizador()
    {
        var torneo = CrearTorneo("COPAAURA");

        _repositorio.Agregar(torneo);
        _baseDeDatos.Contexto.ChangeTracker.Clear();

        Assert.IsGreaterThan(0, torneo.Id);
        var guardado = _baseDeDatos.Contexto.Torneos.SingleOrDefault(t => t.Id == torneo.Id);
        Assert.IsNotNull(guardado);
        Assert.AreEqual("COPAAURA", guardado.Codigo);
        Assert.AreEqual("Copa Aura", guardado.Nombre);
        Assert.AreEqual(Descripcion, guardado.Descripcion);
        Assert.AreEqual(FechaInicio, guardado.FechaInicio);
        Assert.AreEqual(FechaFin, guardado.FechaFin);
        Assert.AreEqual(8, guardado.Cupo);
        Assert.AreEqual(EstadoTorneo.Abierto, guardado.Estado);
        Assert.AreEqual(_organizador.Id, guardado.OrganizadorId);
    }

    [TestMethod]
    public void ObtenerPorId_IdExistente_DevuelveElTorneoConSuOrganizador()
    {
        var otro = CrearTorneo("COPAFUEGO");
        var torneo = CrearTorneo("COPAAURA");
        GuardarTorneos(otro, torneo);

        var resultado = _repositorio.ObtenerPorId(torneo.Id);

        Assert.IsNotNull(resultado);
        Assert.AreEqual(torneo.Id, resultado.Id);
        Assert.AreEqual("COPAAURA", resultado.Codigo);
        Assert.IsNotNull(resultado.Organizador);
        Assert.AreEqual("Laura", resultado.Organizador.Nombre);
    }

    [TestMethod]
    public void ObtenerPorId_IdInexistente_DevuelveNull()
    {
        var resultado = _repositorio.ObtenerPorId(99);

        Assert.IsNull(resultado);
    }

    [TestMethod]
    [DataRow("COPAAURA")]
    [DataRow("copaaura")]
    public void ExistePorCodigo_CodigoGuardado_DevuelveTrueSinDistinguirMayusculas(string codigo)
    {
        GuardarTorneos(CrearTorneo("COPAAURA"));

        var existe = _repositorio.ExistePorCodigo(codigo);

        Assert.IsTrue(existe);
    }

    [TestMethod]
    public void ExistePorCodigo_CodigoInexistente_DevuelveFalse()
    {
        GuardarTorneos(CrearTorneo("COPAAURA"));

        var existe = _repositorio.ExistePorCodigo("COPAFUEGO");

        Assert.IsFalse(existe);
    }

    [TestMethod]
    public void Listar_SinFiltros_DevuelveTodosOrdenadosPorIdConSuOrganizador()
    {
        GuardarTorneos(CrearTorneo("COPAAURA"), CrearTorneo("COPAFUEGO"), CrearTorneo("COPAHIELO"));

        var torneos = _repositorio.Listar(Filtro());

        CollectionAssert.AreEqual(new[] { "COPAAURA", "COPAFUEGO", "COPAHIELO" }, torneos.Select(t => t.Codigo).ToArray());
        Assert.IsTrue(torneos.All(t => t.Organizador.Nombre == "Laura"));
    }

    [TestMethod]
    public void Listar_ConLimiteYDesplazamiento_DevuelveLaPaginaEnOrdenDeId()
    {
        GuardarTorneos(CrearTorneo("COPAAURA"), CrearTorneo("COPAFUEGO"), CrearTorneo("COPAHIELO"), CrearTorneo("COPARAYO"));

        var torneos = _repositorio.Listar(Filtro(limite: 2, desplazamiento: 1));

        CollectionAssert.AreEqual(new[] { "COPAFUEGO", "COPAHIELO" }, torneos.Select(t => t.Codigo).ToArray());
    }

    [TestMethod]
    public void Listar_FiltroPorEstado_DevuelveSoloLosDeEseEstado()
    {
        var cancelado = CrearTorneo("COPAFUEGO");
        cancelado.Cancelar();
        GuardarTorneos(CrearTorneo("COPAAURA"), cancelado);

        var torneos = _repositorio.Listar(Filtro(estado: EstadoTorneo.Cancelado));

        CollectionAssert.AreEqual(new[] { "COPAFUEGO" }, torneos.Select(t => t.Codigo).ToArray());
    }

    [TestMethod]
    public void Listar_ExcluyendoCancelados_OmiteLosTorneosCancelados()
    {
        var cancelado = CrearTorneo("COPAFUEGO");
        cancelado.Cancelar();
        GuardarTorneos(CrearTorneo("COPAAURA"), cancelado, CrearTorneo("COPAHIELO"));

        var torneos = _repositorio.Listar(Filtro(excluirCancelados: true));

        CollectionAssert.AreEqual(new[] { "COPAAURA", "COPAHIELO" }, torneos.Select(t => t.Codigo).ToArray());
    }

    [TestMethod]
    public void Listar_RangoDeFechas_DevuelveLosQueEmpiezanDentroDelRangoIncluyendoLosExtremos()
    {
        GuardarTorneos(
            CrearTorneo("OCTUBRE", fechaInicio: new DateOnly(2026, 10, 31)),
            CrearTorneo("NOVIEMBRE1", fechaInicio: new DateOnly(2026, 11, 1)),
            CrearTorneo("NOVIEMBRE30", fechaInicio: new DateOnly(2026, 11, 30)),
            CrearTorneo("DICIEMBRE", fechaInicio: new DateOnly(2026, 12, 1)));

        var torneos = _repositorio.Listar(Filtro(fechaDesde: new DateOnly(2026, 11, 1), fechaHasta: new DateOnly(2026, 11, 30)));

        CollectionAssert.AreEqual(new[] { "NOVIEMBRE1", "NOVIEMBRE30" }, torneos.Select(t => t.Codigo).ToArray());
    }

    [TestMethod]
    public void Listar_FiltroPorNombre_DevuelveLosQueLoContienenSinDistinguirMayusculas()
    {
        GuardarTorneos(CrearTorneo("COPAAURA", "Copa Aura"), CrearTorneo("LIGAFUEGO", "Liga Fuego"), CrearTorneo("COPAHIELO", "Gran COPA Hielo"));

        var torneos = _repositorio.Listar(Filtro(nombre: "copa"));

        CollectionAssert.AreEqual(new[] { "COPAAURA", "COPAHIELO" }, torneos.Select(t => t.Codigo).ToArray());
    }

    [TestMethod]
    public void Listar_VariosFiltros_DevuelveLosQueCumplenTodos()
    {
        var cancelado = CrearTorneo("COPAFUEGO", "Copa Fuego");
        cancelado.Cancelar();
        GuardarTorneos(
            CrearTorneo("COPAAURA", "Copa Aura"),
            cancelado,
            CrearTorneo("LIGAAURA", "Liga Aura"),
            CrearTorneo("COPAHIELO", "Copa Hielo", new DateOnly(2027, 1, 10)));

        var torneos = _repositorio.Listar(Filtro(
            estado: EstadoTorneo.Abierto,
            fechaHasta: new DateOnly(2026, 12, 31),
            nombre: "copa",
            excluirCancelados: true));

        CollectionAssert.AreEqual(new[] { "COPAAURA" }, torneos.Select(t => t.Codigo).ToArray());
    }

    [TestMethod]
    public void Actualizar_TorneoModificado_GuardaLosCambios()
    {
        var torneo = GuardarYCargar(CrearTorneo("COPAAURA"));
        torneo.Modificar("COPAFUEGO", "Copa Fuego", Descripcion, FechaInicio, FechaFin, 16, torneo.Organizador, FechaActual);

        _repositorio.Actualizar(torneo);
        _baseDeDatos.Contexto.ChangeTracker.Clear();

        var guardado = _baseDeDatos.Contexto.Torneos.Single(t => t.Id == torneo.Id);
        Assert.AreEqual("COPAFUEGO", guardado.Codigo);
        Assert.AreEqual("Copa Fuego", guardado.Nombre);
        Assert.AreEqual(16, guardado.Cupo);
    }

    [TestMethod]
    public void Actualizar_TorneoCancelado_GuardaElEstado()
    {
        var torneo = GuardarYCargar(CrearTorneo("COPAAURA"));
        torneo.Cancelar();

        _repositorio.Actualizar(torneo);
        _baseDeDatos.Contexto.ChangeTracker.Clear();

        Assert.AreEqual(EstadoTorneo.Cancelado, _baseDeDatos.Contexto.Torneos.Single(t => t.Id == torneo.Id).Estado);
    }

    [TestMethod]
    public void Agregar_CodigoOcupadoDespuesDeConsultar_LanzaCodigoTorneoYaExisteSinDuplicar()
    {
        GuardarTorneos(CrearTorneo("COPAAURA"));
        var nuevo = CrearTorneo("COPAAURA", "Otra copa");

        Assert.ThrowsExactly<CodigoTorneoYaExisteException>(() => _repositorio.Agregar(nuevo));
        Assert.AreEqual(1, _baseDeDatos.Contexto.Torneos.Count(t => t.Codigo == "COPAAURA"));
        AssertPuedeGuardarOtroTorneo();
    }

    [TestMethod]
    public void Agregar_ErrorDeBaseSinCodigoDuplicado_PropagaDbUpdateException()
    {
        var existente = CrearTorneo("COPAAURA");
        GuardarTorneos(existente);
        var nuevo = new Torneo("COPAFUEGO", "Copa Fuego", Descripcion, FechaInicio, FechaFin, 8, _organizador, FechaActual)
        {
            Id = existente.Id,
        };

        Assert.ThrowsExactly<DbUpdateException>(() => _repositorio.Agregar(nuevo));
        Assert.IsFalse(_baseDeDatos.Contexto.Torneos.Any(t => t.Codigo == "COPAFUEGO"));
        AssertPuedeGuardarOtroTorneo();
    }

    [TestMethod]
    public void Actualizar_CodigoOcupadoDespuesDeConsultar_LanzaCodigoTorneoYaExisteSinCambiarElTorneo()
    {
        GuardarTorneos(CrearTorneo("COPAFUEGO"));
        var torneo = GuardarYCargar(CrearTorneo("COPAAURA"));
        torneo.Modificar("COPAFUEGO", "Copa Aura", Descripcion, FechaInicio, FechaFin, 8, torneo.Organizador, FechaActual);

        Assert.ThrowsExactly<CodigoTorneoYaExisteException>(() => _repositorio.Actualizar(torneo));
        Assert.AreEqual("COPAAURA", _baseDeDatos.Contexto.Torneos.AsNoTracking().Single(t => t.Id == torneo.Id).Codigo);
    }

    // Arma el filtro del repositorio indicando solo lo que cada test necesita; el resto no filtra.
    private static FiltroTorneos Filtro(
        EstadoTorneo? estado = null,
        DateOnly? fechaDesde = null,
        DateOnly? fechaHasta = null,
        string? nombre = null,
        bool excluirCancelados = false,
        int limite = 20,
        int desplazamiento = 0)
    {
        return new FiltroTorneos(estado, fechaDesde, fechaHasta, nombre, null, excluirCancelados, limite, desplazamiento);
    }

    private void AssertPuedeGuardarOtroTorneo()
    {
        var siguiente = CrearTorneo("SIGUIENTE");

        _repositorio.Agregar(siguiente);

        Assert.IsTrue(_baseDeDatos.Contexto.Torneos.Any(t => t.Id == siguiente.Id && t.Codigo == "SIGUIENTE"));
    }

    // Deja el torneo guardado y cargado desde el contexto, como lo recibe el servicio antes de modificarlo.
    // No usa ObtenerPorId para que estos tests no dependan de otro método del repositorio (FIRST).
    private Torneo GuardarYCargar(Torneo torneo)
    {
        GuardarTorneos(torneo);
        return _baseDeDatos.Contexto.Torneos.Include(t => t.Organizador).Single(t => t.Id == torneo.Id);
    }

    private void GuardarTorneos(params Torneo[] torneos)
    {
        _baseDeDatos.Contexto.Torneos.AddRange(torneos);
        _baseDeDatos.Contexto.SaveChanges();
        _baseDeDatos.Contexto.ChangeTracker.Clear();
    }

    private Torneo CrearTorneo(string codigo, string nombre = "Copa Aura", DateOnly? fechaInicio = null)
    {
        var inicio = fechaInicio ?? FechaInicio;
        return new Torneo(codigo, nombre, Descripcion, inicio, inicio.AddDays(29), 8, _organizador, FechaActual);
    }

    private Usuario GuardarOrganizador()
    {
        var organizador = new Usuario("Laura", "Gomez", "laura@farmeort.com", "laurag", new DateOnly(1990, 5, 20), Rol.Organizador, "hash", FechaActual);
        _baseDeDatos.Contexto.Usuarios.Add(organizador);
        _baseDeDatos.Contexto.SaveChanges();
        return organizador;
    }
}
