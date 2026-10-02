using FarmeORT.Dominio.Batallas;
using FarmeORT.Dominio.Inscripciones;
using FarmeORT.Dominio.Skills;
using FarmeORT.Dominio.Torneos;
using FarmeORT.Dominio.Usuarios;
using FarmeORT.DTOs.Torneos;
using FarmeORT.Excepciones;
using FarmeORT.IRepositorios.Batallas;
using FarmeORT.IRepositorios.Inscripciones;
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
    private Mock<IRepositorioInscripciones> _repositorioInscripciones = null!;
    private Mock<IRepositorioBatallas> _repositorioBatallas = null!;
    private Mock<TimeProvider> _reloj = null!;
    private Usuario _organizador = null!;
    private ServicioTorneos _servicio = null!;

    [TestInitialize]
    public void Inicializar()
    {
        _repositorioTorneos = new Mock<IRepositorioTorneos>(MockBehavior.Strict);
        _repositorioUsuarios = new Mock<IRepositorioUsuarios>(MockBehavior.Strict);
        _repositorioInscripciones = new Mock<IRepositorioInscripciones>(MockBehavior.Strict);
        _repositorioBatallas = new Mock<IRepositorioBatallas>(MockBehavior.Strict);
        _reloj = new Mock<TimeProvider>(MockBehavior.Strict);
        _organizador = CrearUsuario(OrganizadorId, Rol.Organizador);

        // Por defecto el torneo no tiene inscriptos; los tests que lo necesitan redefinen la cantidad.
        _repositorioInscripciones.Setup(r => r.ContarAceptadas(TorneoId)).Returns(0);
        _repositorioInscripciones.Setup(r => r.ContarAceptadasPorTorneo(new[] { TorneoId })).Returns(new Dictionary<int, int>());

        // Por defecto el torneo no tiene batallas sin resolver ni inscripciones Aceptadas; los tests de la
        // cancelación agregan las suyas.
        _repositorioBatallas.Setup(r => r.ListarSinResolverDelTorneo(TorneoId)).Returns([]);

        // Ni rondas: el test del cuadro agrega las suyas.
        _repositorioBatallas.Setup(r => r.ListarDelTorneo(TorneoId)).Returns([]);
        _repositorioInscripciones.Setup(r => r.ListarAceptadasDelTorneo(TorneoId)).Returns([]);
        _servicio = new ServicioTorneos(
            _repositorioTorneos.Object,
            _repositorioUsuarios.Object,
            _repositorioInscripciones.Object,
            _repositorioBatallas.Object,
            _reloj.Object);
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
    public void ObtenerPorId_TorneoCanceladoConsultadoPorAnonimo_LanzaTorneoNoEncontrado()
    {
        var torneo = CrearTorneo();
        torneo.Cancelar();
        _repositorioTorneos.Setup(r => r.ObtenerPorId(TorneoId)).Returns(torneo);

        Assert.ThrowsExactly<TorneoNoEncontradoException>(() => _servicio.ObtenerPorId(TorneoId, esAnonimo: true));
    }

    [TestMethod]
    public void ObtenerPorId_TorneoCanceladoConsultadoPorUsuarioAutenticado_DevuelveSuDetalle()
    {
        var torneo = CrearTorneo();
        torneo.Cancelar();
        _repositorioTorneos.Setup(r => r.ObtenerPorId(TorneoId)).Returns(torneo);

        var response = _servicio.ObtenerPorId(TorneoId, esAnonimo: false);

        Assert.AreEqual(nameof(EstadoTorneo.Cancelado), response.Estado);
    }

    [TestMethod]
    public void Listar_ConFiltros_DevuelveLosTorneosDelRepositorio()
    {
        var desde = new DateOnly(2026, 11, 1);
        var hasta = new DateOnly(2026, 12, 31);
        var filtro = new FiltroTorneos(EstadoTorneo.Abierto, desde, hasta, "copa", 3, false, 10, 5);
        _repositorioTorneos.Setup(r => r.Listar(filtro)).Returns([CrearTorneo()]);
        var request = new FiltroTorneosRequest
        {
            Estado = nameof(EstadoTorneo.Abierto),
            FechaDesde = desde,
            FechaHasta = hasta,
            Nombre = "copa",
            SkillObligatoriaId = 3,
            Limit = 10,
            Offset = 5,
        };

        var torneos = _servicio.Listar(request, esAnonimo: false);

        Assert.HasCount(1, torneos);
        Assert.AreEqual(TorneoId, torneos[0].Id);
        Assert.AreEqual(Codigo, torneos[0].Codigo);
        Assert.AreEqual("Laura Gomez", torneos[0].Organizador);
    }

    [TestMethod]
    public void Listar_ConsultaAnonima_PideExcluirLosTorneosCancelados()
    {
        var filtro = new FiltroTorneos(EstadoTorneo.Cancelado, null, null, null, null, true, 20, 0);
        _repositorioTorneos.Setup(r => r.Listar(filtro)).Returns([]);

        var torneos = _servicio.Listar(new FiltroTorneosRequest { Estado = nameof(EstadoTorneo.Cancelado) }, esAnonimo: true);

        Assert.IsEmpty(torneos);
    }

    [TestMethod]
    [DataRow("Pausado")]
    [DataRow("abierto")]
    [DataRow("1")]
    public void Listar_EstadoInexistente_NoFiltraPorEstado(string estado)
    {
        _repositorioTorneos.Setup(r => r.Listar(new FiltroTorneos(null, null, null, null, null, false, 20, 0))).Returns([]);

        var torneos = _servicio.Listar(new FiltroTorneosRequest { Estado = estado }, esAnonimo: false);

        Assert.IsEmpty(torneos);
    }

    [TestMethod]
    [DataRow(600, -3, 100, 0)]
    [DataRow(0, 0, 20, 0)]
    public void Listar_PaginacionFueraDeRango_LaAjusta(int limit, int offset, int limiteEsperado, int desplazamientoEsperado)
    {
        var filtro = new FiltroTorneos(null, null, null, null, null, false, limiteEsperado, desplazamientoEsperado);
        _repositorioTorneos.Setup(r => r.Listar(filtro)).Returns([]);

        var torneos = _servicio.Listar(new FiltroTorneosRequest { Limit = limit, Offset = offset }, esAnonimo: false);

        Assert.IsEmpty(torneos);
    }

    [TestMethod]
    public void Modificar_DatosValidos_GuardaElTorneoModificadoYLoDevuelve()
    {
        var torneo = CrearTorneo();
        var otroOrganizador = CrearUsuario(3, Rol.Organizador);
        _repositorioTorneos.Setup(r => r.ObtenerPorId(TorneoId)).Returns(torneo);
        _repositorioUsuarios.Setup(r => r.ObtenerPorId(3)).Returns(otroOrganizador);
        _repositorioTorneos.Setup(r => r.Actualizar(torneo));
        ConfigurarFechaActual();
        var request = new TorneoRequest
        {
            Codigo = Codigo,
            Nombre = "Copa Aura Invierno",
            Descripcion = "Edición de invierno del torneo de aura.",
            FechaInicio = new DateOnly(2026, 12, 1),
            FechaFin = new DateOnly(2026, 12, 20),
            Cupo = 16,
            OrganizadorId = 3,
        };

        var response = _servicio.Modificar(TorneoId, request);

        _repositorioTorneos.Verify(
            r => r.Actualizar(It.Is<Torneo>(t =>
                t == torneo
                && t.Nombre == "Copa Aura Invierno"
                && t.Descripcion == "Edición de invierno del torneo de aura."
                && t.FechaInicio == new DateOnly(2026, 12, 1)
                && t.FechaFin == new DateOnly(2026, 12, 20)
                && t.Cupo == 16
                && t.Organizador == otroOrganizador
                && t.OrganizadorId == 3)),
            Times.Once);
        Assert.AreEqual("Copa Aura Invierno", response.Nombre);
        Assert.AreEqual(16, response.Cupo);
        Assert.AreEqual(3, response.OrganizadorId);
    }

    [TestMethod]
    public void Modificar_TorneoInexistente_LanzaTorneoNoEncontradoSinGuardar()
    {
        _repositorioTorneos.Setup(r => r.ObtenerPorId(99)).Returns((Torneo?)null);

        Assert.ThrowsExactly<TorneoNoEncontradoException>(() => _servicio.Modificar(99, CrearRequest()));
        _repositorioTorneos.Verify(r => r.Actualizar(It.IsAny<Torneo>()), Times.Never);
    }

    [TestMethod]
    public void Modificar_OrganizadorInexistente_LanzaUsuarioNoEncontradoSinGuardar()
    {
        _repositorioTorneos.Setup(r => r.ObtenerPorId(TorneoId)).Returns(CrearTorneo());
        _repositorioUsuarios.Setup(r => r.ObtenerPorId(99)).Returns((Usuario?)null);

        Assert.ThrowsExactly<UsuarioNoEncontradoException>(() => _servicio.Modificar(TorneoId, CrearRequest(organizadorId: 99)));
        _repositorioTorneos.Verify(r => r.Actualizar(It.IsAny<Torneo>()), Times.Never);
    }

    [TestMethod]
    public void Modificar_CodigoDeOtroTorneo_LanzaCodigoTorneoYaExisteSinGuardar()
    {
        _repositorioTorneos.Setup(r => r.ObtenerPorId(TorneoId)).Returns(CrearTorneo());
        _repositorioUsuarios.Setup(r => r.ObtenerPorId(OrganizadorId)).Returns(_organizador);
        _repositorioTorneos.Setup(r => r.ExistePorCodigo("COPAFUEGO")).Returns(true);

        Assert.ThrowsExactly<CodigoTorneoYaExisteException>(() => _servicio.Modificar(TorneoId, CrearRequest(codigo: "COPAFUEGO")));
        _repositorioTorneos.Verify(r => r.Actualizar(It.IsAny<Torneo>()), Times.Never);
    }

    [TestMethod]
    public void Modificar_CodigoNuevoLibre_GuardaElTorneoConElCodigoNuevo()
    {
        var torneo = CrearTorneo();
        _repositorioTorneos.Setup(r => r.ObtenerPorId(TorneoId)).Returns(torneo);
        _repositorioUsuarios.Setup(r => r.ObtenerPorId(OrganizadorId)).Returns(_organizador);
        _repositorioTorneos.Setup(r => r.ExistePorCodigo("COPAFUEGO")).Returns(false);
        _repositorioTorneos.Setup(r => r.Actualizar(torneo));
        ConfigurarFechaActual();

        var response = _servicio.Modificar(TorneoId, CrearRequest(codigo: "COPAFUEGO"));

        Assert.AreEqual("COPAFUEGO", response.Codigo);
        _repositorioTorneos.Verify(r => r.Actualizar(It.Is<Torneo>(t => t.Codigo == "COPAFUEGO")), Times.Once);
    }

    [TestMethod]
    public void Modificar_SuMismoCodigoConOtrasMayusculas_NoLoConsideraDuplicado()
    {
        var torneo = CrearTorneo();
        _repositorioTorneos.Setup(r => r.ObtenerPorId(TorneoId)).Returns(torneo);
        _repositorioUsuarios.Setup(r => r.ObtenerPorId(OrganizadorId)).Returns(_organizador);
        _repositorioTorneos.Setup(r => r.Actualizar(torneo));
        ConfigurarFechaActual();

        var response = _servicio.Modificar(TorneoId, CrearRequest(codigo: "CopaAura"));

        Assert.AreEqual("CopaAura", response.Codigo);
        _repositorioTorneos.Verify(r => r.ExistePorCodigo(It.IsAny<string>()), Times.Never);
    }

    [TestMethod]
    [DataRow(nameof(TorneoRequest.FechaInicio), "La fecha de inicio es obligatoria.")]
    [DataRow(nameof(TorneoRequest.FechaFin), "La fecha de fin es obligatoria.")]
    [DataRow(nameof(TorneoRequest.Cupo), "El cupo es obligatorio.")]
    [DataRow(nameof(TorneoRequest.OrganizadorId), "El organizador es obligatorio.")]
    public void Modificar_DatoObligatorioOmitido_LanzaDatosInvalidosSinConsultarLaBase(string campo, string mensaje)
    {
        var request = CrearRequestSin(campo);

        var excepcion = Assert.ThrowsExactly<DatosInvalidosException>(() => _servicio.Modificar(TorneoId, request));

        Assert.AreEqual(mensaje, excepcion.Message);
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

    [TestMethod]
    public void Cancelar_TorneoInexistente_LanzaTorneoNoEncontradoSinGuardar()
    {
        _repositorioTorneos.Setup(r => r.ObtenerPorId(99)).Returns((Torneo?)null);

        Assert.ThrowsExactly<TorneoNoEncontradoException>(() => _servicio.Cancelar(99));
        _repositorioTorneos.Verify(r => r.Actualizar(It.IsAny<Torneo>()), Times.Never);
    }

    [TestMethod]
    public void ObtenerPorId_TorneoConInscripciones_DevuelveLaCantidadDeAceptadas()
    {
        var torneo = CrearTorneo();
        _repositorioTorneos.Setup(r => r.ObtenerPorId(TorneoId)).Returns(torneo);
        _repositorioInscripciones.Setup(r => r.ListarAceptadasDelTorneo(TorneoId)).Returns(
        [
            new Inscripcion(torneo, CrearUsuario(7, Rol.Jugador), new DateTime(2026, 10, 1, 12, 0, 0)),
            new Inscripcion(torneo, CrearUsuario(8, Rol.Jugador), new DateTime(2026, 10, 1, 12, 5, 0)),
            new Inscripcion(torneo, CrearUsuario(9, Rol.Jugador), new DateTime(2026, 10, 1, 12, 10, 0)),
        ]);

        var response = _servicio.ObtenerPorId(TorneoId, esAnonimo: false);

        Assert.AreEqual(3, response.InscripcionesAceptadas);
    }

    [TestMethod]
    public void Listar_TorneosConInscripciones_DevuelveLaCantidadDeAceptadasDeCadaUno()
    {
        var filtro = new FiltroTorneos(null, null, null, null, null, false, 20, 0);
        _repositorioTorneos.Setup(r => r.Listar(filtro)).Returns([CrearTorneo()]);
        _repositorioInscripciones.Setup(r => r.ContarAceptadasPorTorneo(new[] { TorneoId }))
            .Returns(new Dictionary<int, int> { [TorneoId] = 2 });

        var torneos = _servicio.Listar(new FiltroTorneosRequest(), esAnonimo: false);

        Assert.AreEqual(2, torneos.Single().InscripcionesAceptadas);
    }

    [TestMethod]
    public void Modificar_TorneoConInscripciones_DevuelveLaCantidadDeAceptadas()
    {
        var torneo = CrearTorneo();
        _repositorioTorneos.Setup(r => r.ObtenerPorId(TorneoId)).Returns(torneo);
        _repositorioUsuarios.Setup(r => r.ObtenerPorId(OrganizadorId)).Returns(_organizador);
        _repositorioTorneos.Setup(r => r.Actualizar(torneo));
        _repositorioInscripciones.Setup(r => r.ContarAceptadas(TorneoId)).Returns(5);
        ConfigurarFechaActual();

        var response = _servicio.Modificar(TorneoId, CrearRequest());

        Assert.AreEqual(5, response.InscripcionesAceptadas);
    }

    [TestMethod]
    public void Cancelar_TorneoConInscripcionesAceptadas_DevuelveCeroAceptadasSinVolverAContarlas()
    {
        var torneo = CrearTorneo();
        _repositorioTorneos.Setup(r => r.ObtenerPorId(TorneoId)).Returns(torneo);
        _repositorioInscripciones.Setup(r => r.ListarAceptadasDelTorneo(TorneoId)).Returns(
        [
            new Inscripcion(torneo, CrearUsuario(7, Rol.Jugador), new DateTime(2026, 10, 1, 12, 0, 0)),
            new Inscripcion(torneo, CrearUsuario(8, Rol.Jugador), new DateTime(2026, 10, 1, 12, 5, 0)),
        ]);
        _repositorioTorneos.Setup(r => r.Actualizar(torneo));

        var response = _servicio.Cancelar(TorneoId);

        Assert.AreEqual(0, response.InscripcionesAceptadas);
        _repositorioInscripciones.Verify(r => r.ContarAceptadas(It.IsAny<int>()), Times.Never);
    }

    [TestMethod]
    public void Cancelar_TorneoConBatallaSinResolver_CancelaLaBatallaEnElMismoGuardado()
    {
        var torneo = CrearTorneo();
        var batalla = CrearBatalla(torneo);
        EstadoBatalla? estadoAlGuardar = null;
        _repositorioTorneos.Setup(r => r.ObtenerPorId(TorneoId)).Returns(torneo);
        _repositorioBatallas.Setup(r => r.ListarSinResolverDelTorneo(TorneoId)).Returns([batalla]);
        _repositorioTorneos.Setup(r => r.Actualizar(torneo)).Callback(() => estadoAlGuardar = batalla.Estado);

        _servicio.Cancelar(TorneoId);

        Assert.AreEqual(EstadoBatalla.Cancelada, estadoAlGuardar);
        _repositorioTorneos.Verify(r => r.Actualizar(torneo), Times.Once);
    }

    [TestMethod]
    public void Cancelar_TorneoConInscripcionAceptada_CancelaLaInscripcionEnElMismoGuardado()
    {
        var torneo = CrearTorneo();
        var inscripcion = new Inscripcion(torneo, CrearUsuario(7, Rol.Jugador), new DateTime(2026, 10, 1, 12, 0, 0));
        EstadoInscripcion? estadoAlGuardar = null;
        _repositorioTorneos.Setup(r => r.ObtenerPorId(TorneoId)).Returns(torneo);
        _repositorioInscripciones.Setup(r => r.ListarAceptadasDelTorneo(TorneoId)).Returns([inscripcion]);
        _repositorioTorneos.Setup(r => r.Actualizar(torneo)).Callback(() => estadoAlGuardar = inscripcion.Estado);

        _servicio.Cancelar(TorneoId);

        Assert.AreEqual(EstadoInscripcion.Cancelada, estadoAlGuardar);
        _repositorioTorneos.Verify(r => r.Actualizar(torneo), Times.Once);
    }

    [TestMethod]
    public void Cancelar_TorneoYaCancelado_LanzaTorneoNoCancelableSinTocarBatallasNiInscripciones()
    {
        var torneo = CrearTorneo();
        torneo.Cancelar();
        _repositorioTorneos.Setup(r => r.ObtenerPorId(TorneoId)).Returns(torneo);

        Assert.ThrowsExactly<TorneoNoCancelableException>(() => _servicio.Cancelar(TorneoId));

        _repositorioBatallas.Verify(r => r.ListarSinResolverDelTorneo(It.IsAny<int>()), Times.Never);
        _repositorioInscripciones.Verify(r => r.ListarAceptadasDelTorneo(It.IsAny<int>()), Times.Never);
        _repositorioTorneos.Verify(r => r.Actualizar(It.IsAny<Torneo>()), Times.Never);
    }

    [TestMethod]
    public void ObtenerPorId_TorneoConSkillsObligatorias_DevuelveLasSkillsObligatorias()
    {
        var torneo = CrearTorneo();
        torneo.AsociarSkillObligatoria(new Skill("SIXSEVEN", "Six-Seven", "Gesto alternando las manos hacia arriba y abajo.", CategoriaSkill.Gestual, 2) { Id = 3 });
        _repositorioTorneos.Setup(r => r.ObtenerPorId(TorneoId)).Returns(torneo);

        var response = _servicio.ObtenerPorId(TorneoId, esAnonimo: true);

        Assert.HasCount(1, response.SkillsObligatorias);
        var obligatoria = response.SkillsObligatorias.Single();
        Assert.AreEqual(3, obligatoria.Id);
        Assert.AreEqual("SIXSEVEN", obligatoria.Codigo);
        Assert.AreEqual("Six-Seven", obligatoria.Nombre);
        Assert.AreEqual(nameof(CategoriaSkill.Gestual), obligatoria.Categoria);
        Assert.AreEqual(2, obligatoria.Dificultad);
        Assert.IsTrue(obligatoria.Activa);
    }

    [TestMethod]
    public void ObtenerPorId_TorneoConInscriptosAceptados_DevuelveLosJugadoresConNicknameYEloEnOrdenDeInscripcion()
    {
        var torneo = CrearTorneo();
        var primero = new Usuario("Ana", "Perez", "ana@farmeort.com", "aurafarmer", new DateOnly(2000, 1, 1), Rol.Jugador, "hash", FechaActual) { Id = 7 };
        var segundo = new Usuario("Beto", "Diaz", "beto@farmeort.com", "sixseven", new DateOnly(2000, 1, 1), Rol.Jugador, "hash", FechaActual) { Id = 8 };
        _repositorioTorneos.Setup(r => r.ObtenerPorId(TorneoId)).Returns(torneo);
        _repositorioInscripciones.Setup(r => r.ListarAceptadasDelTorneo(TorneoId)).Returns(
        [
            new Inscripcion(torneo, primero, new DateTime(2026, 10, 1, 12, 0, 0)),
            new Inscripcion(torneo, segundo, new DateTime(2026, 10, 1, 12, 5, 0)),
        ]);

        var response = _servicio.ObtenerPorId(TorneoId, esAnonimo: true);

        CollectionAssert.AreEqual(new[] { 7, 8 }, response.JugadoresAceptados.Select(j => j.JugadorId).ToArray());
        CollectionAssert.AreEqual(new[] { "aurafarmer", "sixseven" }, response.JugadoresAceptados.Select(j => j.Nickname).ToArray());
        CollectionAssert.AreEqual(new[] { 1200, 1200 }, response.JugadoresAceptados.Select(j => j.Elo).ToArray());
    }

    [TestMethod]
    public void ObtenerPorId_TorneoConRondas_DevuelveElCuadroConLasBatallasDeCadaRonda()
    {
        var torneo = CrearTorneo();
        var jugadores = Enumerable.Range(7, 4).Select(id => CrearUsuario(id, Rol.Jugador)).ToList();
        var sorteo = new Mock<ISorteo>(MockBehavior.Strict);
        sorteo.Setup(s => s.ElegirEntre(jugadores[0], jugadores[3])).Returns(jugadores[0]);
        sorteo.Setup(s => s.ElegirEntre(jugadores[1], jugadores[2])).Returns(jugadores[1]);
        sorteo.Setup(s => s.ElegirEntre(jugadores[0], jugadores[1])).Returns(jugadores[0]);
        IReadOnlyList<Batalla> batallas =
        [
            new Batalla(torneo, 1, jugadores[0], jugadores[3], sorteo.Object) { Id = 10 },
            new Batalla(torneo, 1, jugadores[1], jugadores[2], sorteo.Object) { Id = 11 },
            new Batalla(torneo, 2, jugadores[0], jugadores[1], sorteo.Object) { Id = 12 },
        ];
        _repositorioTorneos.Setup(r => r.ObtenerPorId(TorneoId)).Returns(torneo);
        _repositorioBatallas.Setup(r => r.ListarDelTorneo(TorneoId)).Returns(batallas);

        var response = _servicio.ObtenerPorId(TorneoId, esAnonimo: true);

        CollectionAssert.AreEqual(new[] { 1, 2 }, response.Rondas.Select(r => r.Numero).ToArray());
        CollectionAssert.AreEqual(new[] { 10, 11 }, response.Rondas[0].Batallas.Select(b => b.Id).ToArray());
        CollectionAssert.AreEqual(new[] { 12 }, response.Rondas[1].Batallas.Select(b => b.Id).ToArray());
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

    private static Batalla CrearBatalla(Torneo torneo)
    {
        var jugador1 = CrearUsuario(7, Rol.Jugador);
        var jugador2 = CrearUsuario(8, Rol.Jugador);
        var sorteo = new Mock<ISorteo>(MockBehavior.Strict);
        sorteo.Setup(s => s.ElegirEntre(jugador1, jugador2)).Returns(jugador1);
        return new Batalla(torneo, 1, jugador1, jugador2, sorteo.Object);
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
