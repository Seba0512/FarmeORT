namespace FarmeORT.Dominio.Elo;

public sealed class CalculadorEloBase : ICalculadorElo
{
    private const int K = 32;
    private const int Victoria = 1;
    private const int Derrota = 0;
    private const int EloMinimo = 100;

    public ResultadoElo Calcular(int eloGanador, int eloPerdedor)
    {
        var nuevoEloGanador = CalcularNuevoElo(eloGanador, eloPerdedor, Victoria);
        var nuevoEloPerdedor = CalcularNuevoElo(eloPerdedor, eloGanador, Derrota);
        return new ResultadoElo(nuevoEloGanador, nuevoEloPerdedor);
    }

    private static int CalcularNuevoElo(int elo, int eloRival, int resultado)
    {
        var puntajeEsperado = 1 / (1 + Math.Pow(10, (eloRival - elo) / 400.0));
        var nuevoElo = (int)Math.Round(elo + (K * (resultado - puntajeEsperado)));
        if(nuevoElo < EloMinimo)
        {
            return EloMinimo;
        }

        return nuevoElo;
    }
}
