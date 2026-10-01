using FarmeORT.DTOs.Skills;
using FarmeORT.DTOs.Usuarios;
using FarmeORT.IServicios.Skills;
using FarmeORT.WebAPI.Filtros;
using Microsoft.AspNetCore.Mvc;

namespace FarmeORT.WebAPI.Controllers;

[ApiController]
[Route("api/v1/skills")]
public sealed class SkillsController(IServicioSkills servicioSkills) : ControllerBase
{
    [HttpPost]
    [Autorizacion(Roles.Administrador)]
    public IActionResult Crear([FromBody] SkillRequest request)
    {
        var response = servicioSkills.Crear(request);
        return Created($"/api/v1/skills/{response.Id}", response);
    }

    [HttpGet]
    [Autenticacion]
    public IActionResult Listar([FromQuery] FiltroSkillsRequest filtro)
    {
        return Ok(servicioSkills.Listar(filtro));
    }

    [HttpGet("{id:int}")]
    [Autenticacion]
    public IActionResult ObtenerPorId(int id)
    {
        return Ok(servicioSkills.ObtenerPorId(id));
    }

    [HttpPut("{id:int}")]
    [Autorizacion(Roles.Administrador)]
    public IActionResult Modificar(int id, [FromBody] SkillRequest request)
    {
        return Ok(servicioSkills.Modificar(id, request));
    }

    [HttpPatch("{id:int}")]
    [Autorizacion(Roles.Administrador)]
    public IActionResult CambiarEstado(int id, [FromBody] SkillEstadoRequest request)
    {
        return Ok(servicioSkills.CambiarEstado(id, request));
    }
}
