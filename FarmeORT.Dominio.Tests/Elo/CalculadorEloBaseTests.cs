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
}
