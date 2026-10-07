using FarmeORT.Servicios.Seguridad;

namespace FarmeORT.Servicios.Tests.Seguridad;

[TestClass]
public sealed class HasheadorContrasenasTests
{
    private const string Contrasena = "Clave123!";

    private HasheadorContrasenas _hasheador = null!;

    [TestInitialize]
    public void Inicializar()
    {
        _hasheador = new HasheadorContrasenas();
    }

    [TestMethod]
    public void Verificar_ContrasenaHasheada_DevuelveTrue()
    {
        var hash = _hasheador.Hashear(Contrasena);

        var resultado = _hasheador.Verificar(Contrasena, hash);

        Assert.IsTrue(resultado);
    }

    [TestMethod]
    public void Verificar_OtraContrasena_DevuelveFalse()
    {
        var hash = _hasheador.Hashear(Contrasena);

        var resultado = _hasheador.Verificar("OtraClave456!", hash);

        Assert.IsFalse(resultado);
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("sin-separador")]
    [DataRow("no-es-base64.tampoco-es-base64")]
    [DataRow("c2FsLXZhbGlkYQ==.no-es-base64")]
    public void Verificar_HashConFormatoInvalido_DevuelveFalse(string hashInvalido)
    {
        var resultado = _hasheador.Verificar(Contrasena, hashInvalido);

        Assert.IsFalse(resultado);
    }

    [TestMethod]
    public void Hashear_MismaContrasenaDosVeces_DevuelveHashesDistintos()
    {
        var primero = _hasheador.Hashear(Contrasena);
        var segundo = _hasheador.Hashear(Contrasena);

        Assert.AreNotEqual(primero, segundo);
    }
}
