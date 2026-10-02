namespace FarmeORT.Dominio.Elo;

public interface ICalculadorElo
{
    ResultadoElo Calcular(int eloGanador, int eloPerdedor);
}
