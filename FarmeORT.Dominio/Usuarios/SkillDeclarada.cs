using FarmeORT.Dominio.Skills;

namespace FarmeORT.Dominio.Usuarios;

public sealed class SkillDeclarada
{
    public SkillDeclarada(Usuario jugador, Skill skill, int nivel, DateOnly fechaDeclaracion)
    {
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
