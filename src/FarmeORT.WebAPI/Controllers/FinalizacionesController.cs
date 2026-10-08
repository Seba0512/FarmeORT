using FarmeORT.DTOs.Usuarios;
using FarmeORT.IServicios.Finalizaciones;
using FarmeORT.WebAPI.Filtros;
using Microsoft.AspNetCore.Mvc;

namespace FarmeORT.WebAPI.Controllers;

[ApiController]
[Route("api/v1/torneos/{torneoId:int}/finalizaciones")]
public sealed class FinalizacionesController(IServicioFinalizaciones servicioFinalizaciones) : ControllerBase
{
    [HttpPost]
    [Autorizacion(Roles.Organizador, Roles.Administrador)]
    public IActionResult Finalizar(int torneoId)
    {
        var sesion = HttpContext.ObtenerSesionAutenticada();
        return Ok(servicioFinalizaciones.Finalizar(torneoId, sesion.UsuarioId, sesion.Rol));
    }
}
