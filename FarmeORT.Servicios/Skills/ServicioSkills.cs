using FarmeORT.DTOs.Skills;
using FarmeORT.IRepositorios.Skills;
using FarmeORT.IServicios.Skills;

namespace FarmeORT.Servicios.Skills;

internal sealed class ServicioSkills(IRepositorioSkills repositorioSkills) : IServicioSkills
{
    public SkillResponse Crear(SkillRequest request)
    {
        _ = repositorioSkills;
        _ = request;
        throw new NotImplementedException();
    }

    public SkillResponse ObtenerPorId(int id)
    {
        _ = id;
        throw new NotImplementedException();
    }

    public IReadOnlyList<SkillResponse> Listar(FiltroSkillsRequest filtro)
    {
        _ = filtro;
        throw new NotImplementedException();
    }

    public SkillResponse Modificar(int id, SkillRequest request)
    {
        _ = id;
        _ = request;
        throw new NotImplementedException();
    }

    public SkillResponse CambiarEstado(int id, SkillEstadoRequest request)
    {
        _ = id;
        _ = request;
        throw new NotImplementedException();
    }
}
