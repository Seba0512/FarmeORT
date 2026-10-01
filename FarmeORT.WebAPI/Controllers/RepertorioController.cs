using FarmeORT.DTOs.Repertorio;
using FarmeORT.IServicios.Repertorio;
using FarmeORT.WebAPI.Filtros;
using Microsoft.AspNetCore.Mvc;

namespace FarmeORT.WebAPI.Controllers;

[ApiController]
[Route("api/v1/jugadores/actual/repertorio")]
public sealed class RepertorioController(IServicioRepertorio servicioRepertorio) : ControllerBase
{
    [HttpGet]
    public IActionResult Listar()
    {
        var sesion = HttpContext.ObtenerSesionAutenticada();
        return Ok(servicioRepertorio.Listar(sesion.UsuarioId));
    }

    [HttpPost]
    public IActionResult Declarar([FromBody] SkillDeclaradaRequest request)
    {
        var sesion = HttpContext.ObtenerSesionAutenticada();
        var response = servicioRepertorio.Declarar(sesion.UsuarioId, request);
        return Created($"/api/v1/jugadores/actual/repertorio/{response.SkillId}", response);
    }

    [HttpDelete("{skillId:int}")]
    public IActionResult Quitar(int skillId)
    {
        _ = skillId;
        throw new NotImplementedException();
    }
}
