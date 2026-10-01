using FarmeORT.DTOs.Skills;
using FarmeORT.DTOs.Torneos;
using FarmeORT.IRepositorios.Skills;
using FarmeORT.IRepositorios.Torneos;
using FarmeORT.IServicios.Torneos;

namespace FarmeORT.Servicios.Torneos;

internal sealed class ServicioSkillsObligatorias(
    IRepositorioTorneos repositorioTorneos,
    IRepositorioSkills repositorioSkills) : IServicioSkillsObligatorias
{
    public IReadOnlyList<SkillResponse> Listar(int torneoId, bool esAnonimo)
    {
        _ = torneoId;
        _ = esAnonimo;
        throw new NotImplementedException();
    }

    public SkillResponse Asociar(int torneoId, SkillObligatoriaRequest request, int organizadorId)
    {
        _ = repositorioTorneos;
        _ = repositorioSkills;
        _ = torneoId;
        _ = request;
        _ = organizadorId;
        throw new NotImplementedException();
    }

    public void Quitar(int torneoId, int skillId, int organizadorId)
    {
        _ = torneoId;
        _ = skillId;
        _ = organizadorId;
        throw new NotImplementedException();
    }
}
