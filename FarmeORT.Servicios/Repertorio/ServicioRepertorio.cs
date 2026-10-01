using FarmeORT.DTOs.Repertorio;
using FarmeORT.IRepositorios.Skills;
using FarmeORT.IRepositorios.Usuarios;
using FarmeORT.IServicios.Repertorio;

namespace FarmeORT.Servicios.Repertorio;

internal sealed class ServicioRepertorio(
    IRepositorioUsuarios repositorioUsuarios,
    IRepositorioSkills repositorioSkills,
    TimeProvider reloj) : IServicioRepertorio
{
    public IReadOnlyList<SkillDeclaradaResponse> Listar(int jugadorId)
    {
        _ = jugadorId;
        throw new NotImplementedException();
    }

    public SkillDeclaradaResponse Declarar(int jugadorId, SkillDeclaradaRequest request)
    {
        _ = repositorioUsuarios;
        _ = repositorioSkills;
        _ = reloj;
        _ = jugadorId;
        _ = request;
        throw new NotImplementedException();
    }

    public void Quitar(int jugadorId, int skillId)
    {
        _ = jugadorId;
        _ = skillId;
        throw new NotImplementedException();
    }
}
