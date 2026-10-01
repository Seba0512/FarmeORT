using FarmeORT.DTOs.Torneos;
using FarmeORT.IServicios.Torneos;
using FarmeORT.WebAPI.Filtros;
using Microsoft.AspNetCore.Mvc;

namespace FarmeORT.WebAPI.Controllers;

[ApiController]
[Route("api/v1/torneos/{torneoId:int}/skills-obligatorias")]
public sealed class SkillsObligatoriasController(IServicioSkillsObligatorias servicioSkillsObligatorias) : ControllerBase
{
    [HttpGet]
    public IActionResult Listar(int torneoId)
    {
        _ = torneoId;
        throw new NotImplementedException();
    }

    [HttpPost]
    public IActionResult Asociar(int torneoId, [FromBody] SkillObligatoriaRequest request)
    {
        var sesion = HttpContext.ObtenerSesionAutenticada();
        var response = servicioSkillsObligatorias.Asociar(torneoId, request, sesion.UsuarioId);
        return Created($"/api/v1/torneos/{torneoId}/skills-obligatorias/{response.Id}", response);
    }
}
