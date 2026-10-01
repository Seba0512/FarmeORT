using FarmeORT.DTOs.Torneos;
using FarmeORT.IServicios.Torneos;
using Microsoft.AspNetCore.Mvc;

namespace FarmeORT.WebAPI.Controllers;

[ApiController]
[Route("api/v1/torneos")]
public sealed class TorneosController(IServicioTorneos servicioTorneos) : ControllerBase
{
    [HttpPost]
    public IActionResult Crear([FromBody] TorneoRequest request)
    {
        _ = servicioTorneos;
        _ = request;
        throw new NotImplementedException();
    }
}
