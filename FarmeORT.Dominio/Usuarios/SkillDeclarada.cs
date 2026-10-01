using FarmeORT.Dominio.Skills;

namespace FarmeORT.Dominio.Usuarios;

public sealed class SkillDeclarada
{
    public SkillDeclarada(Usuario jugador, Skill skill, int nivel, DateOnly fechaDeclaracion)
    {
        _ = jugador;
        _ = skill;
        _ = nivel;
        _ = fechaDeclaracion;
        throw new NotImplementedException();
    }

    public int JugadorId { get; private set; }

    public int SkillId { get; private set; }

    public Skill Skill { get; private set; }

    public int Nivel { get; private set; }

    public DateOnly FechaDeclaracion { get; private set; }
}
