using FarmeORT.Dominio.Usuarios;
using FarmeORT.Excepciones;

namespace FarmeORT.Dominio.Tests.Usuarios;

[TestClass]
public sealed class UsuarioTests
{
    private static readonly DateOnly FechaActual = new(2026, 9, 30);

    [TestMethod]
    [DataRow(100)]
    [DataRow(1216)]
    public void ActualizarElo_ValorValido_ConservaNuevoElo(int elo)
    {
        var usuario = CrearUsuario();

        usuario.ActualizarElo(elo);

        Assert.AreEqual(elo, usuario.Elo);
    }

    [TestMethod]
    [DataRow(-1)]
    [DataRow(0)]
    [DataRow(99)]
    public void ActualizarElo_MenorAlPiso_RechazaSinModificar(int elo)
    {
        var usuario = CrearUsuario();

        Assert.ThrowsExactly<DatosInvalidosException>(() => usuario.ActualizarElo(elo));

        Assert.AreEqual(1200, usuario.Elo);
    }

    [TestMethod]
    public void Constructor_DatosValidos_ConservaLosDatosYEloInicial()
    {
        var fechaNacimiento = new DateOnly(2000, 1, 15);

        var usuario = new Usuario("Ana", "Pérez", "ana@farmeort.com", "anaperez", fechaNacimiento, Rol.Jugador, "hash-ana", FechaActual);

        Assert.AreEqual("Ana", usuario.Nombre);
        Assert.AreEqual("Pérez", usuario.Apellido);
        Assert.AreEqual("ana@farmeort.com", usuario.Email);
        Assert.AreEqual("anaperez", usuario.Nickname);
        Assert.AreEqual(fechaNacimiento, usuario.FechaNacimiento);
        Assert.AreEqual(Rol.Jugador, usuario.Rol);
        Assert.AreEqual("hash-ana", usuario.ContrasenaHash);
        Assert.AreEqual(1200, usuario.Elo);
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("   ")]
    public void Constructor_NombreVacio_LanzaDatosInvalidos(string nombre)
    {
        Assert.ThrowsExactly<DatosInvalidosException>(() => CrearUsuario(nombre: nombre));
    }

    [TestMethod]
    public void Constructor_NombreDeMasDe50Caracteres_LanzaDatosInvalidos()
    {
        var nombre = "NombreDeCincuentaYUnCaracteresParaProbarElLimiteXYZ";

        Assert.ThrowsExactly<DatosInvalidosException>(() => CrearUsuario(nombre: nombre));
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("Pe")]
    [DataRow("ApellidoDeVeintiseisLetras")]
    public void Constructor_ApellidoFueraDeRango_LanzaDatosInvalidos(string apellido)
    {
        Assert.ThrowsExactly<DatosInvalidosException>(() => CrearUsuario(apellido: apellido));
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("no-es-un-email")]
    [DataRow("ana@")]
    [DataRow("Ana <ana@farmeort.com>")]
    public void Constructor_EmailInvalido_LanzaDatosInvalidos(string email)
    {
        Assert.ThrowsExactly<DatosInvalidosException>(() => CrearUsuario(email: email));
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("ab")]
    [DataRow("NicknameDeVeintiunoXX")]
    [DataRow("con espacio")]
    [DataRow("con-guion")]
    public void Constructor_NicknameInvalido_LanzaDatosInvalidos(string nickname)
    {
        Assert.ThrowsExactly<DatosInvalidosException>(() => CrearUsuario(nickname: nickname));
    }

    [TestMethod]
    public void Constructor_MenorDe13Anios_LanzaDatosInvalidos()
    {
        var cumple13Maniana = FechaActual.AddYears(-13).AddDays(1);

        Assert.ThrowsExactly<DatosInvalidosException>(() => CrearUsuario(fechaNacimiento: cumple13Maniana));
    }

    [TestMethod]
    public void Constructor_Cumple13AniosHoy_EsValido()
    {
        var cumple13Hoy = FechaActual.AddYears(-13);

        var usuario = CrearUsuario(fechaNacimiento: cumple13Hoy);

        Assert.AreEqual(cumple13Hoy, usuario.FechaNacimiento);
    }

    [TestMethod]
    public void Constructor_FechaNacimientoMaxima_LanzaDatosInvalidos()
    {
        Assert.ThrowsExactly<DatosInvalidosException>(() => CrearUsuario(fechaNacimiento: DateOnly.MaxValue));
    }

    [TestMethod]
    [DataRow(2026, 10, 1)]
    [DataRow(9999, 12, 31)]
    public void Constructor_FechaFutura_ExplicaQueElNacimientoNoPuedeSerFuturo(int anio, int mes, int dia)
    {
        var excepcion = Assert.ThrowsExactly<DatosInvalidosException>(() => CrearUsuario(fechaNacimiento: new DateOnly(anio, mes, dia)));

        Assert.AreEqual("La fecha de nacimiento no puede ser futura.", excepcion.Message);
    }

    [TestMethod]
    public void Modificar_DatosValidos_ReemplazaLosDatosYConservaIdYElo()
    {
        var usuario = new Usuario("Ana", "Pérez", "ana@farmeort.com", "anaperez", new DateOnly(2000, 1, 15), Rol.Organizador, "hash-ana", FechaActual)
        {
            Id = 5,
        };

        usuario.Modificar("Carla", "Gómez", "carla@farmeort.com", "carlagomez", new DateOnly(1998, 6, 3), Rol.Jugador, "hash-carla", FechaActual);

        Assert.AreEqual(5, usuario.Id);
        Assert.AreEqual("Carla", usuario.Nombre);
        Assert.AreEqual("Gómez", usuario.Apellido);
        Assert.AreEqual("carla@farmeort.com", usuario.Email);
        Assert.AreEqual("carlagomez", usuario.Nickname);
        Assert.AreEqual(new DateOnly(1998, 6, 3), usuario.FechaNacimiento);
        Assert.AreEqual(Rol.Jugador, usuario.Rol);
        Assert.AreEqual("hash-carla", usuario.ContrasenaHash);
        Assert.AreEqual(1200, usuario.Elo);
    }

    [TestMethod]
    [DataRow("", "Gómez", "carla@farmeort.com", "carlagomez", 1998)]
    [DataRow("Carla", "Go", "carla@farmeort.com", "carlagomez", 1998)]
    [DataRow("Carla", "Gómez", "no-es-un-email", "carlagomez", 1998)]
    [DataRow("Carla", "Gómez", "carla@farmeort.com", "carla gomez", 1998)]
    [DataRow("Carla", "Gómez", "carla@farmeort.com", "carlagomez", 2020)]
    public void Modificar_DatosInvalidos_LanzaDatosInvalidosSinCambiarElUsuario(
        string nombre,
        string apellido,
        string email,
        string nickname,
        int anioNacimiento)
    {
        var usuario = CrearUsuario();

        Assert.ThrowsExactly<DatosInvalidosException>(
            () => usuario.Modificar(nombre, apellido, email, nickname, new DateOnly(anioNacimiento, 6, 3), Rol.Organizador, "hash-carla", FechaActual));

        Assert.AreEqual("Ana", usuario.Nombre);
        Assert.AreEqual("Pérez", usuario.Apellido);
        Assert.AreEqual("ana@farmeort.com", usuario.Email);
        Assert.AreEqual("anaperez", usuario.Nickname);
        Assert.AreEqual(new DateOnly(2000, 1, 15), usuario.FechaNacimiento);
        Assert.AreEqual(Rol.Jugador, usuario.Rol);
        Assert.AreEqual("hash-ana", usuario.ContrasenaHash);
    }

    [TestMethod]
    public void Eliminar_UsuarioActivo_QuedaEliminadoConservandoSusDatos()
    {
        var usuario = CrearUsuario();

        usuario.Eliminar();

        Assert.IsTrue(usuario.Eliminado);
        Assert.AreEqual("ana@farmeort.com", usuario.Email);
        Assert.AreEqual("anaperez", usuario.Nickname);
    }

    private static Usuario CrearUsuario(
        string nombre = "Ana",
        string apellido = "Pérez",
        string email = "ana@farmeort.com",
        string nickname = "anaperez",
        DateOnly? fechaNacimiento = null)
    {
        var nacimiento = fechaNacimiento ?? new DateOnly(2000, 1, 15);
        return new Usuario(nombre, apellido, email, nickname, nacimiento, Rol.Jugador, "hash-ana", FechaActual);
    }
}
