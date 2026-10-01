using FarmeORT.Dominio.Skills;
using FarmeORT.Dominio.Usuarios;

namespace FarmeORT.Dominio.Batallas;

public sealed class Lanzamiento
{
    public Lanzamiento(Batalla batalla, int turno, Usuario jugador, Skill skill, DateTimeOffset fecha)
    {
        throw new NotImplementedException();
    }

    public int Id { get; init; }

    public int BatallaId { get; private set; }

    public Batalla Batalla { get; private set; }

    public int Turno { get; private set; }

    public int JugadorId { get; private set; }

    public Usuario Jugador { get; private set; }

    public int SkillId { get; private set; }

    public Skill Skill { get; private set; }

    public DateTimeOffset Fecha { get; private set; }
}
