using FarmeORT.DTOs.Usuarios;
using FarmeORT.IServicios.Usuarios;
using Microsoft.AspNetCore.Mvc;

namespace FarmeORT.WebAPI.Controllers;

[ApiController]
[Route("api/v1/jugadores")]
public sealed class JugadoresController(IServicioUsuarios servicioUsuarios) : ControllerBase
{
    [HttpPost]
    public IActionResult Registrar([FromBody] RegistroJugadorRequest request)
    {
        var response = servicioUsuarios.RegistrarJugador(request);
        return Created($"/api/v1/usuarios/{response.Id}", response);
    }
}
