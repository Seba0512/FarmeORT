using FarmeORT.DTOs.Torneos;
using FarmeORT.IServicios.Torneos;
using Microsoft.AspNetCore.Mvc;

namespace FarmeORT.WebAPI.Controllers;

[ApiController]
[Route("api/v1/torneos/{torneoId:int}/skills-obligatorias")]
public sealed class SkillsObligatoriasController(IServicioSkillsObligatorias servicioSkillsObligatorias) : ControllerBase
{
    [HttpPost]
    public IActionResult Asociar(int torneoId, [FromBody] SkillObligatoriaRequest request)
    {
        _ = servicioSkillsObligatorias;
        _ = torneoId;
        _ = request;
        throw new NotImplementedException();
    }
}
