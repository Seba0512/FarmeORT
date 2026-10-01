using FarmeORT.DTOs.Inscripciones;
using FarmeORT.IServicios.Inscripciones;
using Microsoft.AspNetCore.Mvc;

namespace FarmeORT.WebAPI.Controllers;

[ApiController]
[Route("api/v1/inscripciones")]
public sealed class InscripcionesController(IServicioInscripciones servicioInscripciones) : ControllerBase
{
    [HttpPost]
    public IActionResult Inscribir([FromBody] InscripcionRequest request)
    {
        _ = servicioInscripciones;
        _ = request;
        throw new NotImplementedException();
    }
}
