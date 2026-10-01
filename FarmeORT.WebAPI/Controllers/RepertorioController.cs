using FarmeORT.DTOs.Repertorio;
using FarmeORT.IServicios.Repertorio;
using FarmeORT.WebAPI.Filtros;
using Microsoft.AspNetCore.Mvc;

namespace FarmeORT.WebAPI.Controllers;

[ApiController]
[Route("api/v1/jugadores/actual/repertorio")]
public sealed class RepertorioController(IServicioRepertorio servicioRepertorio) : ControllerBase
{
    [HttpPost]
    public IActionResult Declarar([FromBody] SkillDeclaradaRequest request)
    {
        var sesion = HttpContext.ObtenerSesionAutenticada();
        var response = servicioRepertorio.Declarar(sesion.UsuarioId, request);
        return Created($"/api/v1/jugadores/actual/repertorio/{response.SkillId}", response);
    }
}
