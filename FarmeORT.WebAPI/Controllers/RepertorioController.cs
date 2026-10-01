using FarmeORT.DTOs.Repertorio;
using FarmeORT.DTOs.Usuarios;
using FarmeORT.IServicios.Repertorio;
using FarmeORT.WebAPI.Filtros;
using Microsoft.AspNetCore.Mvc;

namespace FarmeORT.WebAPI.Controllers;

[ApiController]
[Route("api/v1/jugadores/actual/repertorio")]
[Autorizacion(Roles.Jugador)]
public sealed class RepertorioController(IServicioRepertorio servicioRepertorio) : ControllerBase
{
    // "actual" es el jugador del token: el id nunca llega en la ruta ni en el body.
    private int JugadorActualId => HttpContext.ObtenerSesionAutenticada().UsuarioId;

    [HttpGet]
    public IActionResult Listar()
    {
        return Ok(servicioRepertorio.Listar(JugadorActualId));
    }

    [HttpPost]
    public IActionResult Declarar([FromBody] SkillDeclaradaRequest request)
    {
        var response = servicioRepertorio.Declarar(JugadorActualId, request);
        return Created($"/api/v1/jugadores/actual/repertorio/{response.SkillId}", response);
    }

    [HttpDelete("{skillId:int}")]
    public IActionResult Quitar(int skillId)
    {
        servicioRepertorio.Quitar(JugadorActualId, skillId);
        return NoContent();
    }
}
