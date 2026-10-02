namespace FarmeORT.Servicios;

// Convierte el texto de un filtro en un valor del enum comparando por nombre exacto: un número como "1" o un
// nombre que no existe devuelven null, y ese filtro no se aplica (D-35).
internal static class ValoresPorNombre
{
    public static TEnum? Buscar<TEnum>(string nombre)
        where TEnum : struct, Enum
    {
        foreach(var valor in Enum.GetValues<TEnum>())
        {
            if(valor.ToString() == nombre)
            {
                return valor;
            }
        }

        return null;
    }
}
