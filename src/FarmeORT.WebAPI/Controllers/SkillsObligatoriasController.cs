using FarmeORT.DTOs.Torneos;
using FarmeORT.DTOs.Usuarios;
using FarmeORT.IServicios.Torneos;
using FarmeORT.WebAPI.Filtros;
using Microsoft.AspNetCore.Mvc;

namespace FarmeORT.WebAPI.Controllers;

[ApiController]
[Route("api/v1/torneos/{torneoId:int}/skills-obligatorias")]
public sealed class SkillsObligatoriasController(IServicioSkillsObligatorias servicioSkillsObligatorias) : ControllerBase
{
    [HttpGet]
    [AutenticacionOpcional]
    public IActionResult Listar(int torneoId)
    {
        var esAnonimo = HttpContext.EsAnonimo();
        return Ok(servicioSkillsObligatorias.Listar(torneoId, esAnonimo));
    }

    [HttpPost]
    [Autorizacion(Roles.Organizador)]
    public IActionResult Asociar(int torneoId, [FromBody] SkillObligatoriaRequest request)
    {
        var sesion = HttpContext.ObtenerSesionAutenticada();
        var response = servicioSkillsObligatorias.Asociar(torneoId, request, sesion.UsuarioId);
        return Created($"/api/v1/torneos/{torneoId}/skills-obligatorias/{response.Id}", response);
    }

    [HttpDelete("{skillId:int}")]
    [Autorizacion(Roles.Organizador)]
    public IActionResult Quitar(int torneoId, int skillId)
    {
        var sesion = HttpContext.ObtenerSesionAutenticada();
        servicioSkillsObligatorias.Quitar(torneoId, skillId, sesion.UsuarioId);
        return NoContent();
    }
}
