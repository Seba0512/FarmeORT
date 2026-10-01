using FarmeORT.Dominio.Torneos;
using FarmeORT.Dominio.Usuarios;
using FarmeORT.IRepositorios.Torneos;
using FarmeORT.Repositorios.Torneos;

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
