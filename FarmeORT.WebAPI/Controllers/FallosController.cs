using FarmeORT.DTOs.Batallas;
using FarmeORT.IServicios.Batallas;
using FarmeORT.WebAPI.Filtros;
using Microsoft.AspNetCore.Mvc;

namespace FarmeORT.WebAPI.Controllers;

[ApiController]
[Route("api/v1/batallas/{id:int}/fallos")]
public sealed class FallosController(IServicioFallos servicioFallos) : ControllerBase
{
    [HttpPost]
    public IActionResult Fallar(int id, [FromBody] FalloRequest request)
    {
        var organizadorId = HttpContext.ObtenerSesionAutenticada().UsuarioId;
        var response = servicioFallos.Fallar(id, organizadorId, request);
        return Created($"/api/v1/batallas/{id}", response);
    }
}
