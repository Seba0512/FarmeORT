using FarmeORT.DTOs.Repertorio;

namespace FarmeORT.IServicios.Repertorio;

public interface IServicioRepertorio
{
    SkillDeclaradaResponse Declarar(int jugadorId, SkillDeclaradaRequest request);
}
