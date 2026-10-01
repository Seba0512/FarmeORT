using FarmeORT.Dominio.Skills;
using FarmeORT.Excepciones;

namespace FarmeORT.Dominio.Usuarios;

public sealed class SkillDeclarada
{
    private const int NivelMinimo = 1;
    private const int NivelMaximo = 5;

    public SkillDeclarada(Usuario jugador, Skill skill, int nivel, DateOnly fechaDeclaracion)
    {
        if(nivel < NivelMinimo || nivel > NivelMaximo)
        {
            throw new DatosInvalidosException("El nivel debe ser un entero entre 1 y 5.");
        }

        JugadorId = jugador.Id;
        SkillId = skill.Id;
        Skill = skill;
        Nivel = nivel;
        FechaDeclaracion = fechaDeclaracion;
    }

    // EF Core materializa la skill declarada con este constructor y completa la navegación después (D-08).
    private SkillDeclarada()
    {
        Skill = null!;
    }

    public int JugadorId { get; private set; }

    public int SkillId { get; private set; }

    public Skill Skill { get; private set; }

    public int Nivel { get; private set; }

    public DateOnly FechaDeclaracion { get; private set; }
}
