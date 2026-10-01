using FarmeORT.Dominio.Torneos;
using FarmeORT.Dominio.Usuarios;
using FarmeORT.DTOs.Torneos;
using FarmeORT.Excepciones;
using FarmeORT.IRepositorios.Torneos;
using FarmeORT.IRepositorios.Usuarios;
using FarmeORT.Servicios.Torneos;
using Moq;

namespace FarmeORT.Servicios.Tests.Torneos;

[TestClass]
public sealed class ServicioTorneosTests
{
    private const int TorneoId = 4;
    private const int OrganizadorId = 2;
    private const string Codigo = "COPAAURA";
    private const string Nombre = "Copa Aura";
    private const string Descripcion = "Torneo eliminatorio de batallas de aura.";
    private const int Cupo = 8;

    private static readonly DateOnly FechaActual = new(2026, 10, 1);
    private static readonly DateOnly FechaInicio = new(2026, 11, 1);
    private static readonly DateOnly FechaFin = new(2026, 11, 30);

    private Mock<IRepositorioTorneos> _repositorioTorneos = null!;
    private Mock<IRepositorioUsuarios> _repositorioUsuarios = null!;
    private Mock<TimeProvider> _reloj = null!;
    private Usuario _organizador = null!;
    private ServicioTorneos _servicio = null!;

    [TestInitialize]
    public void Inicializar()
    {
        _repositorioTorneos = new Mock<IRepositorioTorneos>(MockBehavior.Strict);
        _repositorioUsuarios = new Mock<IRepositorioUsuarios>(MockBehavior.Strict);
        _reloj = new Mock<TimeProvider>(MockBehavior.Strict);
        _organizador = CrearUsuario(OrganizadorId, Rol.Organizador);
        _servicio = new ServicioTorneos(_repositorioTorneos.Object, _repositorioUsuarios.Object, _reloj.Object);
    }

    [TestMethod]
    public void Crear_DatosValidos_GuardaElTorneoAbiertoConSuOrganizador()
    {
        ConfigurarAltaValida();

        _servicio.Crear(CrearRequest());

        _repositorioTorneos.Verify(
            r => r.Agregar(It.Is<Torneo>(t =>
                t.Codigo == Codigo
                && t.Nombre == Nombre
                && t.Descripcion == Descripcion
                && t.FechaInicio == FechaInicio
                && t.FechaFin == FechaFin
                && t.Cupo == Cupo
                && t.Estado == EstadoTorneo.Abierto
                && t.Organizador == _organizador
                && t.OrganizadorId == OrganizadorId)),
            Times.Once);
    }

    [TestMethod]
    public void Crear_DatosValidos_DevuelveElTorneoSinInscripciones()
    {
        ConfigurarAltaValida();

        var response = _servicio.Crear(CrearRequest());

        Assert.AreEqual(Codigo, response.Codigo);
        Assert.AreEqual(Nombre, response.Nombre);
        Assert.AreEqual(Descripcion, response.Descripcion);
        Assert.AreEqual(nameof(EstadoTorneo.Abierto), response.Estado);
        Assert.AreEqual(FechaInicio, response.FechaInicio);
        Assert.AreEqual(FechaFin, response.FechaFin);
        Assert.AreEqual(Cupo, response.Cupo);
        Assert.AreEqual(0, response.InscripcionesAceptadas);
        Assert.AreEqual(OrganizadorId, response.OrganizadorId);
        Assert.AreEqual("Laura Gomez", response.Organizador);
    }

    [TestMethod]
    public void Crear_CodigoExistente_LanzaCodigoTorneoYaExisteSinGuardar()
    {
        _repositorioTorneos.Setup(r => r.ExistePorCodigo(Codigo)).Returns(true);

        Assert.ThrowsExactly<CodigoTorneoYaExisteException>(() => _servicio.Crear(CrearRequest()));
        _repositorioTorneos.Verify(r => r.Agregar(It.IsAny<Torneo>()), Times.Never);
    }

    [TestMethod]
    public void Crear_OrganizadorInexistente_LanzaUsuarioNoEncontradoSinGuardar()
    {
        _repositorioTorneos.Setup(r => r.ExistePorCodigo(Codigo)).Returns(false);
        _repositorioUsuarios.Setup(r => r.ObtenerPorId(99)).Returns((Usuario?)null);

        Assert.ThrowsExactly<UsuarioNoEncontradoException>(() => _servicio.Crear(CrearRequest(organizadorId: 99)));
        _repositorioTorneos.Verify(r => r.Agregar(It.IsAny<Torneo>()), Times.Never);
    }

    [TestMethod]
    [DataRow(nameof(TorneoRequest.FechaInicio), "La fecha de inicio es obligatoria.")]
    [DataRow(nameof(TorneoRequest.FechaFin), "La fecha de fin es obligatoria.")]
    [DataRow(nameof(TorneoRequest.Cupo), "El cupo es obligatorio.")]
    [DataRow(nameof(TorneoRequest.OrganizadorId), "El organizador es obligatorio.")]
    public void Crear_DatoObligatorioOmitido_LanzaDatosInvalidosSinConsultarLaBase(string campo, string mensaje)
    {
        var request = CrearRequestSin(campo);

        var excepcion = Assert.ThrowsExactly<DatosInvalidosException>(() => _servicio.Crear(request));

        Assert.AreEqual(mensaje, excepcion.Message);
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void ObtenerPorId_TorneoAbierto_DevuelveSuDetalle(bool esAnonimo)
    {
        _repositorioTorneos.Setup(r => r.ObtenerPorId(TorneoId)).Returns(CrearTorneo());

        var response = _servicio.ObtenerPorId(TorneoId, esAnonimo);

        Assert.AreEqual(TorneoId, response.Id);
        Assert.AreEqual(Codigo, response.Codigo);
        Assert.AreEqual(nameof(EstadoTorneo.Abierto), response.Estado);
        Assert.AreEqual(Cupo, response.Cupo);
        Assert.AreEqual(0, response.InscripcionesAceptadas);
        Assert.AreEqual("Laura Gomez", response.Organizador);
    }

    [TestMethod]
    public void ObtenerPorId_TorneoInexistente_LanzaTorneoNoEncontrado()
    {
        _repositorioTorneos.Setup(r => r.ObtenerPorId(99)).Returns((Torneo?)null);

        Assert.ThrowsExactly<TorneoNoEncontradoException>(() => _servicio.ObtenerPorId(99, esAnonimo: false));
    }

    [TestMethod]
    public void Cancelar_TorneoAbierto_GuardaElTorneoCanceladoYLoDevuelve()
    {
        var torneo = CrearTorneo();
        _repositorioTorneos.Setup(r => r.ObtenerPorId(TorneoId)).Returns(torneo);
        _repositorioTorneos.Setup(r => r.Actualizar(torneo));

        var response = _servicio.Cancelar(TorneoId);

        _repositorioTorneos.Verify(r => r.Actualizar(It.Is<Torneo>(t => t == torneo && t.Estado == EstadoTorneo.Cancelado)), Times.Once);
        Assert.AreEqual(nameof(EstadoTorneo.Cancelado), response.Estado);
    }

    private void ConfigurarAltaValida()
    {
        ConfigurarFechaActual();
        _repositorioTorneos.Setup(r => r.ExistePorCodigo(Codigo)).Returns(false);
        _repositorioUsuarios.Setup(r => r.ObtenerPorId(OrganizadorId)).Returns(_organizador);
        _repositorioTorneos.Setup(r => r.Agregar(It.Is<Torneo>(t => t.Codigo == Codigo)));
    }

    private void ConfigurarFechaActual()
    {
        _reloj.Setup(r => r.GetUtcNow()).Returns(new DateTimeOffset(2026, 10, 1, 12, 0, 0, TimeSpan.Zero));
        _reloj.Setup(r => r.LocalTimeZone).Returns(TimeZoneInfo.Utc);
    }

    private Torneo CrearTorneo()
    {
        return new Torneo(Codigo, Nombre, Descripcion, FechaInicio, FechaFin, Cupo, _organizador, FechaActual)
        {
            Id = TorneoId,
        };
    }

    private static Usuario CrearUsuario(int id, Rol rol)
    {
        return new Usuario("Laura", "Gomez", "laura@farmeort.com", "laurag", new DateOnly(1990, 5, 20), rol, "hash", FechaActual)
        {
            Id = id,
        };
    }

    private static TorneoRequest CrearRequest(string codigo = Codigo, int? organizadorId = OrganizadorId)
    {
        return new TorneoRequest
        {
            Codigo = codigo,
            Nombre = Nombre,
            Descripcion = Descripcion,
            FechaInicio = FechaInicio,
            FechaFin = FechaFin,
            Cupo = Cupo,
            OrganizadorId = organizadorId,
        };
    }

    private static TorneoRequest CrearRequestSin(string campo)
    {
        return new TorneoRequest
        {
            Codigo = Codigo,
            Nombre = Nombre,
            Descripcion = Descripcion,
            FechaInicio = campo == nameof(TorneoRequest.FechaInicio) ? null : FechaInicio,
            FechaFin = campo == nameof(TorneoRequest.FechaFin) ? null : FechaFin,
            Cupo = campo == nameof(TorneoRequest.Cupo) ? null : Cupo,
            OrganizadorId = campo == nameof(TorneoRequest.OrganizadorId) ? null : OrganizadorId,
        };
    }
}
