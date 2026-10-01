using FarmeORT.DTOs.Skills;
using FarmeORT.DTOs.Torneos;

namespace FarmeORT.IServicios.Torneos;

public interface IServicioSkillsObligatorias
{
    SkillResponse Asociar(int torneoId, SkillObligatoriaRequest request, int organizadorId);
}
