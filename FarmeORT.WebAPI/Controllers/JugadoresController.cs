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
        _ = servicioUsuarios;
        _ = request;
        throw new NotImplementedException();
    }
}
