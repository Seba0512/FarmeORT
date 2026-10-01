using FarmeORT.DTOs.Repertorio;

namespace FarmeORT.IServicios.Repertorio;

public interface IServicioRepertorio
{
    IReadOnlyList<SkillDeclaradaResponse> Listar(int jugadorId);

    SkillDeclaradaResponse Declarar(int jugadorId, SkillDeclaradaRequest request);
}
