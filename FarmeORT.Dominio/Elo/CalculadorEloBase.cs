namespace FarmeORT.Dominio.Elo;

public sealed class CalculadorEloBase : ICalculadorElo
{
    public ResultadoElo Calcular(int eloGanador, int eloPerdedor)
    {
        var esperadoGanador = 1 / (1 + Math.Pow(10, (eloPerdedor - eloGanador) / 400.0));
        var esperadoPerdedor = 1 / (1 + Math.Pow(10, (eloGanador - eloPerdedor) / 400.0));
        var nuevoEloGanador = eloGanador + (32 * (1 - esperadoGanador));
        var nuevoEloPerdedor = eloPerdedor + (32 * (0 - esperadoPerdedor));
        return new ResultadoElo((int)Math.Round(nuevoEloGanador), (int)Math.Round(nuevoEloPerdedor));
    }
}
