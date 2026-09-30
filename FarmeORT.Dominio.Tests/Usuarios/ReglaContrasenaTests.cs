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
}
