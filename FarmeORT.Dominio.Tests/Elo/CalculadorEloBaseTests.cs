using FarmeORT.Dominio.Elo;

namespace FarmeORT.Dominio.Tests.Elo;

[TestClass]
public sealed class CalculadorEloBaseTests
{
    private CalculadorEloBase _calculador = null!;

    [TestInitialize]
    public void Inicializar()
    {
        _calculador = new CalculadorEloBase();
    }

    [TestMethod]
    [DataRow(1200, 1200, 1216, 1184)]
    [DataRow(1500, 1500, 1516, 1484)]
    public void Calcular_EloIguales_GanadorSumaYPerdedorRestaLaMitadDeK(
        int eloGanador,
        int eloPerdedor,
        int nuevoEloGanador,
        int nuevoEloPerdedor)
    {
        var resultado = _calculador.Calcular(eloGanador, eloPerdedor);

        Assert.AreEqual(nuevoEloGanador, resultado.NuevoEloGanador);
        Assert.AreEqual(nuevoEloPerdedor, resultado.NuevoEloPerdedor);
    }

    [TestMethod]
    [DataRow(1400, 1200, 1408, 1192)]
    [DataRow(1200, 1400, 1224, 1376)]
    [DataRow(1600, 1000, 1601, 999)]
    public void Calcular_EloDistintos_CalculaAmbosConLosValoresPreviosYRedondea(
        int eloGanador,
        int eloPerdedor,
        int nuevoEloGanador,
        int nuevoEloPerdedor)
    {
        var resultado = _calculador.Calcular(eloGanador, eloPerdedor);

        Assert.AreEqual(nuevoEloGanador, resultado.NuevoEloGanador);
        Assert.AreEqual(nuevoEloPerdedor, resultado.NuevoEloPerdedor);
    }
}
