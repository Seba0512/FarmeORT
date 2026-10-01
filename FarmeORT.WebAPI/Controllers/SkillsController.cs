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

    [HttpPut("{id:int}")]
    public IActionResult Modificar(int id, [FromBody] SkillRequest request)
    {
        return Ok(servicioSkills.Modificar(id, request));
    }

    [HttpPatch("{id:int}")]
    public IActionResult CambiarEstado(int id, [FromBody] SkillEstadoRequest request)
    {
        _ = id;
        _ = request;
        throw new NotImplementedException();
    }
}
