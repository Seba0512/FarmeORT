using FarmeORT.DTOs.Skills;
using FarmeORT.IServicios.Skills;
using Microsoft.AspNetCore.Mvc;

namespace FarmeORT.WebAPI.Controllers;

[ApiController]
[Route("api/v1/skills")]
public sealed class SkillsController(IServicioSkills servicioSkills) : ControllerBase
{
    [HttpPost]
    public IActionResult Crear([FromBody] SkillRequest request)
    {
        _ = servicioSkills;
        _ = request;
        throw new NotImplementedException();
    }
}
