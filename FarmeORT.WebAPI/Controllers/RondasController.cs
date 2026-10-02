using FarmeORT.IServicios.Rondas;
using Microsoft.AspNetCore.Mvc;

namespace FarmeORT.WebAPI.Controllers;

[ApiController]
[Route("api/v1/torneos/{torneoId:int}/rondas")]
public sealed class RondasController(IServicioRondas servicioRondas) : ControllerBase
{
    [HttpPost]
    public IActionResult Generar(int torneoId)
    {
        _ = servicioRondas;
        _ = torneoId;
        throw new NotImplementedException();
    }
}
