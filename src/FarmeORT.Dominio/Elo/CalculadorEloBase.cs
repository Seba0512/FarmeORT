namespace FarmeORT.Dominio.Elo;

public sealed class CalculadorEloBase : ICalculadorElo
{
    private const int K = 32;
    private const int Victoria = 1;
    private const int Derrota = 0;
    private const int EloMinimo = 100;

    // Con 400 puntos de diferencia, el de mayor ELO tiene 10 veces más chances de ganar (letra 1.10).
    private const double BaseLogistica = 10;
    private const double FactorDeEscala = 400.0;

    public ResultadoElo Calcular(int eloGanador, int eloPerdedor)
    {
        var nuevoEloGanador = CalcularNuevoElo(eloGanador, eloPerdedor, Victoria);
        var nuevoEloPerdedor = CalcularNuevoElo(eloPerdedor, eloGanador, Derrota);
        return new ResultadoElo(nuevoEloGanador, nuevoEloPerdedor);
    }

    private static int CalcularNuevoElo(int elo, int eloRival, int resultado)
    {
        var puntajeEsperado = 1 / (1 + Math.Pow(BaseLogistica, (eloRival - elo) / FactorDeEscala));
        var nuevoElo = (int)Math.Round(elo + (K * (resultado - puntajeEsperado)));
        if(nuevoElo < EloMinimo)
        {
            return EloMinimo;
        }

        return nuevoElo;
    }
}
