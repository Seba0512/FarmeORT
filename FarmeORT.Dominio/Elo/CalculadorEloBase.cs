namespace FarmeORT.Dominio.Elo;

public sealed class CalculadorEloBase : ICalculadorElo
{
    public ResultadoElo Calcular(int eloGanador, int eloPerdedor)
    {
        return new ResultadoElo(eloGanador + 16, eloPerdedor - 16);
    }
}
