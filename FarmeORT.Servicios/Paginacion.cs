namespace FarmeORT.Servicios;

// Ajusta limit y offset fuera de rango en lugar de rechazarlos, "para no trancar al usuario" (AGENTS.md §7).
internal static class Paginacion
{
    private const int LimiteMaximo = 100;
    private const int LimitePorDefecto = 20;

    public static int AjustarLimite(int limite)
    {
        if(limite > LimiteMaximo)
        {
            return LimiteMaximo;
        }

        if(limite < 1)
        {
            return LimitePorDefecto;
        }

        return limite;
    }

    public static int AjustarDesplazamiento(int desplazamiento)
    {
        if(desplazamiento < 0)
        {
            return 0;
        }

        return desplazamiento;
    }
}
