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

    // El token identifica a la sesión; el servicio controla que sea del usuario autenticado.
    [HttpDelete("{token:guid}")]
    [Autenticacion]
    public IActionResult Eliminar(Guid token)
    {
        var sesion = HttpContext.ObtenerSesionAutenticada();
        servicioSesiones.CerrarSesion(token, sesion.UsuarioId);
        return NoContent();
    }
}
