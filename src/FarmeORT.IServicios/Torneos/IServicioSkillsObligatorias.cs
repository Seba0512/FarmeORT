using FarmeORT.DTOs.Skills;
using FarmeORT.DTOs.Torneos;

namespace FarmeORT.IServicios.Torneos;

public interface IServicioSkillsObligatorias
{
    IReadOnlyList<SkillResponse> Listar(int torneoId, bool esAnonimo);

    SkillResponse Asociar(int torneoId, SkillObligatoriaRequest request, int organizadorId);

    void Quitar(int torneoId, int skillId, int organizadorId);
}
