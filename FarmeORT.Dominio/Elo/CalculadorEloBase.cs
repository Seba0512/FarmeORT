namespace FarmeORT.Dominio.Elo;

public sealed class CalculadorEloBase : ICalculadorElo
{
    public ResultadoElo Calcular(int eloGanador, int eloPerdedor)
    {
        _ = eloGanador;
        _ = eloPerdedor;
        throw new NotImplementedException();
    }
}
