using FarmeORT.DTOs.Skills;

namespace FarmeORT.IServicios.Skills;

public interface IServicioSkills
{
    SkillResponse Crear(SkillRequest request);

    SkillResponse ObtenerPorId(int id);

    IReadOnlyList<SkillResponse> Listar(FiltroSkillsRequest filtro);

    SkillResponse Modificar(int id, SkillRequest request);
}
