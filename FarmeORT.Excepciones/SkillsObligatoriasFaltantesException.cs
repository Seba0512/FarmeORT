namespace FarmeORT.Excepciones;

// Los detalles son los códigos de las skills obligatorias que el jugador no tiene declaradas.
public sealed class SkillsObligatoriasFaltantesException(IReadOnlyList<string> codigosFaltantes)
    : ExcepcionConDetalles("No tenés declaradas todas las skills obligatorias del torneo.", codigosFaltantes)
{
}
