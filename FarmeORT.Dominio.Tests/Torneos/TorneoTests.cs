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
