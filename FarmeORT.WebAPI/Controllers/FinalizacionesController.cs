using FarmeORT.IServicios.Finalizaciones;
using Microsoft.AspNetCore.Mvc;

namespace FarmeORT.WebAPI.Controllers;

[ApiController]
[Route("api/v1/torneos/{torneoId:int}/finalizaciones")]
public sealed class FinalizacionesController(IServicioFinalizaciones servicioFinalizaciones) : ControllerBase
{
    [HttpPost]
    public IActionResult Finalizar(int torneoId)
    {
        _ = servicioFinalizaciones;
        _ = torneoId;
        throw new NotImplementedException();
    }
}
