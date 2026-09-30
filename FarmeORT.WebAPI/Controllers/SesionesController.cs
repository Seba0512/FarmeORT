using FarmeORT.DTOs.Sesiones;
using FarmeORT.IServicios.Sesiones;
using FarmeORT.WebAPI.Filtros;
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

    [HttpDelete("actual")]
    public IActionResult Eliminar()
    {
        var sesion = (SesionAutenticada)HttpContext.Items[ClavesContexto.SesionAutenticada]!;
        servicioSesiones.CerrarSesion(sesion.Token);
        return NoContent();
    }
}
