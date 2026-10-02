using FarmeORT.DTOs.Batallas;
using FarmeORT.IServicios.Batallas;
using Microsoft.AspNetCore.Mvc;

namespace FarmeORT.WebAPI.Controllers;

[ApiController]
[Route("api/v1/batallas/{id:int}/fallos")]
public sealed class FallosController(IServicioFallos servicioFallos) : ControllerBase
{
    [HttpPost]
    public IActionResult Fallar(int id, [FromBody] FalloRequest request)
    {
        _ = servicioFallos;
        throw new NotImplementedException();
    }
}
