using FarmeORT.DTOs.Repertorio;
using FarmeORT.IServicios.Repertorio;
using Microsoft.AspNetCore.Mvc;

namespace FarmeORT.WebAPI.Controllers;

[ApiController]
[Route("api/v1/jugadores/actual/repertorio")]
public sealed class RepertorioController(IServicioRepertorio servicioRepertorio) : ControllerBase
{
    [HttpPost]
    public IActionResult Declarar([FromBody] SkillDeclaradaRequest request)
    {
        _ = servicioRepertorio;
        _ = request;
        throw new NotImplementedException();
    }
}
