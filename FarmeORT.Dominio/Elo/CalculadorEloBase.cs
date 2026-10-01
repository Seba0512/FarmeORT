namespace FarmeORT.Dominio.Elo;

public sealed class CalculadorEloBase : ICalculadorElo
{
    private const int K = 32;
    private const int Victoria = 1;
    private const int Derrota = 0;

    public ResultadoElo Calcular(int eloGanador, int eloPerdedor)
    {
        var nuevoEloGanador = CalcularNuevoElo(eloGanador, eloPerdedor, Victoria);
        var nuevoEloPerdedor = CalcularNuevoElo(eloPerdedor, eloGanador, Derrota);
        return new ResultadoElo(nuevoEloGanador, nuevoEloPerdedor);
    }

    private static int CalcularNuevoElo(int elo, int eloRival, int resultado)
    {
        var puntajeEsperado = 1 / (1 + Math.Pow(10, (eloRival - elo) / 400.0));
        var nuevoElo = elo + (K * (resultado - puntajeEsperado));
        return (int)Math.Round(nuevoElo);
    }
}
