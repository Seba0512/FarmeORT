using FarmeORT.DTOs.Batallas;
using FarmeORT.DTOs.Usuarios;
using FarmeORT.IServicios.Batallas;
using FarmeORT.WebAPI.Filtros;
using Microsoft.AspNetCore.Mvc;

namespace FarmeORT.WebAPI.Controllers;

[ApiController]
[Route("api/v1/batallas/{id:int}/lanzamientos")]
[Autorizacion(Roles.Jugador)]
public sealed class LanzamientosController(IServicioLanzamientos servicioLanzamientos) : ControllerBase
{
    [HttpPost]
    public IActionResult Lanzar(int id, [FromBody] LanzamientoRequest request)
    {
        throw new NotImplementedException(servicioLanzamientos.GetType().Name);
    }
}
