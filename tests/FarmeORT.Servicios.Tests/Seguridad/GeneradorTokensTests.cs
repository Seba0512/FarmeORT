using FarmeORT.Servicios.Seguridad;

namespace FarmeORT.Servicios.Tests.Seguridad;

[TestClass]
public sealed class GeneradorTokensTests
{
    [TestMethod]
    public void Generar_DosLlamadas_DevuelveTokensDistintosYNoVacios()
    {
        var generador = new GeneradorTokens();

        var primero = generador.Generar();
        var segundo = generador.Generar();

        Assert.AreNotEqual(Guid.Empty, primero);
        Assert.AreNotEqual(Guid.Empty, segundo);
        Assert.AreNotEqual(primero, segundo);
    }
}
