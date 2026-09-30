using FarmeORT.DTOs.Sesiones;
using FarmeORT.IServicios.Sesiones;
using Microsoft.AspNetCore.Mvc;

namespace FarmeORT.WebAPI.Controllers;

[ApiController]
[Route("api/v1/sesiones")]
public sealed class SesionesController(IServicioSesiones servicioSesiones) : ControllerBase
{
    [HttpPost]
    public IActionResult Crear([FromBody] LoginRequest request)
    {
        var response = servicioSesiones.IniciarSesion(request);
        return Created("/api/v1/sesiones/actual", response);
    }
}
