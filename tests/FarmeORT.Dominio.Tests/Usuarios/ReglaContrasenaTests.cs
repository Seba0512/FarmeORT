using FarmeORT.Dominio.Usuarios;
using FarmeORT.Excepciones;

namespace FarmeORT.Dominio.Tests.Usuarios;

[TestClass]
public sealed class ReglaContrasenaTests
{
    [TestMethod]
    [DataRow("")]
    [DataRow("Corta123!")]
    [DataRow("ClaveDemasiadoLarga12345!!")]
    [DataRow("sinmayusculas123!!")]
    [DataRow("SINMINUSCULAS123!!")]
    [DataRow("SinNumerosPeroLarga!!")]
    [DataRow("SinSimbolos12345Ab")]
    public void Validar_ContrasenaInvalida_LanzaDatosInvalidos(string contrasena)
    {
        Assert.ThrowsExactly<DatosInvalidosException>(() => ReglaContrasena.Validar(contrasena));
    }

    [TestMethod]
    [DataRow("ClaveSegura12!!")]
    [DataRow("ClaveMuySegura12345!!Abcd")]
    public void Validar_ContrasenaValidaEnLosLimites_NoLanza(string contrasena)
    {
        ReglaContrasena.Validar(contrasena);
    }
}
