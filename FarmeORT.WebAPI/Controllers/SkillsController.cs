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
        var response = servicioSkills.Crear(request);
        return Created($"/api/v1/skills/{response.Id}", response);
    }

    [HttpGet]
    public IActionResult Listar([FromQuery] FiltroSkillsRequest filtro)
    {
        return Ok(servicioSkills.Listar(filtro));
    }

    [HttpGet("{id:int}")]
    public IActionResult ObtenerPorId(int id)
    {
        return Ok(servicioSkills.ObtenerPorId(id));
    }
}
