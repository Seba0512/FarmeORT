namespace FarmeORT.Excepciones;

// Los detalles son los nicknames de los inscriptos Aceptados que no tienen declarada la skill (letra 2.4).
public sealed class JugadoresSinSkillObligatoriaException(IReadOnlyList<string> nicknames)
    : ExcepcionConDetalles("Hay jugadores inscriptos que no tienen declarada la skill.", nicknames)
{
}
