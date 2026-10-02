using FarmeORT.IServicios.Rondas;
using FarmeORT.WebAPI.Filtros;
using Microsoft.AspNetCore.Mvc;

namespace FarmeORT.WebAPI.Controllers;

[ApiController]
[Route("api/v1/torneos/{torneoId:int}/rondas")]
public sealed class RondasController(IServicioRondas servicioRondas) : ControllerBase
{
    [HttpPost]
    public IActionResult Generar(int torneoId)
    {
        var sesion = HttpContext.ObtenerSesionAutenticada();
        var response = servicioRondas.Generar(torneoId, sesion.UsuarioId, sesion.Rol);
        return Created($"/api/v1/torneos/{torneoId}/rondas/{response.Numero}", response);
    }
}
