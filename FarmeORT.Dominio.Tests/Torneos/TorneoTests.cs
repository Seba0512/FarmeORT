using FarmeORT.Dominio.Torneos;
using FarmeORT.Dominio.Usuarios;
using FarmeORT.Excepciones;

namespace FarmeORT.Dominio.Tests.Torneos;

[TestClass]
public sealed class TorneoTests
{
    private const string Descripcion = "Torneo eliminatorio de batallas de aura.";

    private static readonly DateOnly FechaActual = new(2026, 10, 1);
    private static readonly DateOnly FechaInicio = new(2026, 11, 1);
    private static readonly DateOnly FechaFin = new(2026, 11, 30);

    [TestMethod]
    public void Constructor_DatosValidos_ConservaLosDatosYQuedaAbierto()
    {
        var organizador = CrearOrganizador(Rol.Organizador);

        var torneo = new Torneo("COPAAURA", "Copa Aura", Descripcion, FechaInicio, FechaFin, 8, organizador, FechaActual);

        Assert.AreEqual("COPAAURA", torneo.Codigo);
        Assert.AreEqual("Copa Aura", torneo.Nombre);
        Assert.AreEqual(Descripcion, torneo.Descripcion);
        Assert.AreEqual(FechaInicio, torneo.FechaInicio);
        Assert.AreEqual(FechaFin, torneo.FechaFin);
        Assert.AreEqual(8, torneo.Cupo);
        Assert.AreSame(organizador, torneo.Organizador);
        Assert.AreEqual(organizador.Id, torneo.OrganizadorId);
        Assert.AreEqual(EstadoTorneo.Abierto, torneo.Estado);
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("COPA")]
    [DataRow("ABCDEFGHIJKLMNOPQRSTU")]
    [DataRow("COPA-AURA")]
    [DataRow("COPA AURA")]
    public void Constructor_CodigoInvalido_LanzaDatosInvalidos(string codigo)
    {
        Assert.ThrowsExactly<DatosInvalidosException>(() => CrearTorneo(codigo: codigo));
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("        ")]
    public void Constructor_NombreVacio_LanzaDatosInvalidos(string nombre)
    {
        Assert.ThrowsExactly<DatosInvalidosException>(() => CrearTorneo(nombre: nombre));
    }

    [TestMethod]
    [DataRow(4)]
    [DataRow(61)]
    public void Constructor_NombreFueraDeRango_LanzaDatosInvalidos(int largo)
    {
        Assert.ThrowsExactly<DatosInvalidosException>(() => CrearTorneo(nombre: new string('a', largo)));
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("                        ")]
    public void Constructor_DescripcionVacia_LanzaDatosInvalidos(string descripcion)
    {
        Assert.ThrowsExactly<DatosInvalidosException>(() => CrearTorneo(descripcion: descripcion));
    }

    [TestMethod]
    [DataRow(19)]
    [DataRow(501)]
    public void Constructor_DescripcionFueraDeRango_LanzaDatosInvalidos(int largo)
    {
        Assert.ThrowsExactly<DatosInvalidosException>(() => CrearTorneo(descripcion: new string('a', largo)));
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(2)]
    [DataRow(6)]
    [DataRow(12)]
    [DataRow(63)]
    [DataRow(128)]
    public void Constructor_CupoQueNoEsPotenciaDeDosEntre4Y64_LanzaDatosInvalidos(int cupo)
    {
        var excepcion = Assert.ThrowsExactly<DatosInvalidosException>(() => CrearTorneo(cupo: cupo));

        Assert.AreEqual("El cupo debe ser una potencia de 2 entre 4 y 64.", excepcion.Message);
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(-1)]
    public void Constructor_FechaFinNoPosteriorAlInicio_LanzaDatosInvalidos(int diasDespuesDelInicio)
    {
        var fechaFin = FechaInicio.AddDays(diasDespuesDelInicio);

        var excepcion = Assert.ThrowsExactly<DatosInvalidosException>(() => CrearTorneo(fechaFin: fechaFin));

        Assert.AreEqual("La fecha de fin debe ser posterior a la de inicio.", excepcion.Message);
    }

    [TestMethod]
    public void Constructor_FechaInicioAnteriorALaActual_LanzaDatosInvalidos()
    {
        var fechaInicio = FechaActual.AddDays(-1);

        var excepcion = Assert.ThrowsExactly<DatosInvalidosException>(() => CrearTorneo(fechaInicio: fechaInicio));

        Assert.AreEqual("La fecha de inicio no puede ser anterior a la fecha actual.", excepcion.Message);
    }

    [TestMethod]
    [DataRow(Rol.Jugador)]
    [DataRow(Rol.Administrador)]
    public void Constructor_OrganizadorSinRolOrganizador_LanzaDatosInvalidos(Rol rol)
    {
        var excepcion = Assert.ThrowsExactly<DatosInvalidosException>(() => CrearTorneo(rolOrganizador: rol));

        Assert.AreEqual("El organizador debe ser un usuario con rol Organizador.", excepcion.Message);
    }

    [TestMethod]
    [DataRow("ABC12", 5, 20, 4)]
    [DataRow("ABCDEFGHIJ0123456789", 60, 500, 64)]
    [DataRow("CopaAura2026", 30, 100, 16)]
    public void Constructor_DatosEnLosLimites_CreaElTorneo(string codigo, int largoNombre, int largoDescripcion, int cupo)
    {
        var torneo = CrearTorneo(codigo, new string('a', largoNombre), new string('a', largoDescripcion), cupo: cupo);

        Assert.AreEqual(codigo, torneo.Codigo);
        Assert.AreEqual(cupo, torneo.Cupo);
    }

    [TestMethod]
    public void Constructor_IniciaHoyYTerminaManana_CreaElTorneo()
    {
        var torneo = CrearTorneo(fechaInicio: FechaActual, fechaFin: FechaActual.AddDays(1));

        Assert.AreEqual(FechaActual, torneo.FechaInicio);
        Assert.AreEqual(FechaActual.AddDays(1), torneo.FechaFin);
    }

    [TestMethod]
    public void Modificar_TorneoCancelado_LanzaTorneoNoModificableSinCambiarlo()
    {
        var torneo = CrearTorneo();
        torneo.Cancelar();

        Assert.ThrowsExactly<TorneoNoModificableException>(
            () => torneo.Modificar("COPAFUEGO", "Copa Fuego", Descripcion, FechaInicio, FechaFin, 16, CrearOrganizador(Rol.Organizador), FechaActual));
        Assert.AreEqual("COPAAURA", torneo.Codigo);
        Assert.AreEqual(8, torneo.Cupo);
    }

    [TestMethod]
    public void Modificar_TorneoEnCurso_LanzaTorneoNoModificableSinCambiarlo()
    {
        var torneo = CrearTorneo();
        torneo.Iniciar(8);

        Assert.ThrowsExactly<TorneoNoModificableException>(
            () => torneo.Modificar("COPAFUEGO", "Copa Fuego", Descripcion, FechaInicio, FechaFin, 16, CrearOrganizador(Rol.Organizador), FechaActual));
        Assert.AreEqual("COPAAURA", torneo.Codigo);
        Assert.AreEqual(EstadoTorneo.EnCurso, torneo.Estado);
    }

    [TestMethod]
    [DataRow("COPA", "Copa Fuego", Descripcion, 16, Rol.Organizador)]
    [DataRow("COPAFUEGO", "Copa", Descripcion, 16, Rol.Organizador)]
    [DataRow("COPAFUEGO", "Copa Fuego", "Muy corta", 16, Rol.Organizador)]
    [DataRow("COPAFUEGO", "Copa Fuego", Descripcion, 12, Rol.Organizador)]
    [DataRow("COPAFUEGO", "Copa Fuego", Descripcion, 16, Rol.Jugador)]
    public void Modificar_DatosInvalidos_LanzaDatosInvalidosSinCambiarElTorneo(
        string codigo,
        string nombre,
        string descripcion,
        int cupo,
        Rol rolOrganizador)
    {
        var torneo = CrearTorneo();

        Assert.ThrowsExactly<DatosInvalidosException>(
            () => torneo.Modificar(codigo, nombre, descripcion, FechaInicio, FechaFin, cupo, CrearOrganizador(rolOrganizador), FechaActual));
        Assert.AreEqual("COPAAURA", torneo.Codigo);
        Assert.AreEqual("Copa Aura", torneo.Nombre);
        Assert.AreEqual(8, torneo.Cupo);
    }

    [TestMethod]
    public void Modificar_FechaFinNoPosteriorAlInicio_LanzaDatosInvalidos()
    {
        var torneo = CrearTorneo();

        Assert.ThrowsExactly<DatosInvalidosException>(
            () => torneo.Modificar("COPAAURA", "Copa Aura", Descripcion, FechaInicio, FechaInicio, 8, CrearOrganizador(Rol.Organizador), FechaActual));
    }

    [TestMethod]
    public void Modificar_FechaInicioNuevaAnteriorALaActual_LanzaDatosInvalidos()
    {
        var torneo = CrearTorneo();
        var fechaPosterior = new DateOnly(2026, 11, 10);

        var excepcion = Assert.ThrowsExactly<DatosInvalidosException>(
            () => torneo.Modificar("COPAAURA", "Copa Aura", Descripcion, new DateOnly(2026, 11, 5), FechaFin, 8, CrearOrganizador(Rol.Organizador), fechaPosterior));

        Assert.AreEqual("La fecha de inicio no puede ser anterior a la fecha actual.", excepcion.Message);
    }

    [TestMethod]
    public void Modificar_FechaInicioSinCambiosYaPasada_ReemplazaLosDemasDatos()
    {
        var torneo = CrearTorneo();
        var fechaPosterior = new DateOnly(2026, 11, 10);
        var otroOrganizador = CrearOrganizador(Rol.Organizador);

        torneo.Modificar("COPAFUEGO", "Copa Fuego", Descripcion, FechaInicio, FechaFin, 16, otroOrganizador, fechaPosterior);

        Assert.AreEqual("COPAFUEGO", torneo.Codigo);
        Assert.AreEqual("Copa Fuego", torneo.Nombre);
        Assert.AreEqual(FechaInicio, torneo.FechaInicio);
        Assert.AreEqual(16, torneo.Cupo);
        Assert.AreSame(otroOrganizador, torneo.Organizador);
        Assert.AreEqual(EstadoTorneo.Abierto, torneo.Estado);
    }

    [TestMethod]
    public void Cancelar_TorneoAbierto_QuedaCancelado()
    {
        var torneo = CrearTorneo();

        torneo.Cancelar();

        Assert.AreEqual(EstadoTorneo.Cancelado, torneo.Estado);
    }

    [TestMethod]
    public void Cancelar_TorneoEnCurso_QuedaCancelado()
    {
        var torneo = CrearTorneo();
        torneo.Iniciar(8);

        torneo.Cancelar();

        Assert.AreEqual(EstadoTorneo.Cancelado, torneo.Estado);
    }

    [TestMethod]
    public void Cancelar_TorneoCancelado_LanzaTorneoNoCancelable()
    {
        var torneo = CrearTorneo();
        torneo.Cancelar();

        Assert.ThrowsExactly<TorneoNoCancelableException>(torneo.Cancelar);
    }

    private static Torneo CrearTorneo(
        string codigo = "COPAAURA",
        string nombre = "Copa Aura",
        string descripcion = Descripcion,
        DateOnly? fechaInicio = null,
        DateOnly? fechaFin = null,
        int cupo = 8,
        Rol rolOrganizador = Rol.Organizador)
    {
        return new Torneo(
            codigo,
            nombre,
            descripcion,
            fechaInicio ?? FechaInicio,
            fechaFin ?? FechaFin,
            cupo,
            CrearOrganizador(rolOrganizador),
            FechaActual);
    }

    private static Usuario CrearOrganizador(Rol rol)
    {
        return new Usuario("Laura", "Gomez", "laura@farmeort.com", "laurag", new DateOnly(1990, 5, 20), rol, "hash", FechaActual)
        {
            Id = 2,
        };
    }
}
