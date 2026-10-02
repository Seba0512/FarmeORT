using FarmeORT.DTOs.Usuarios;
using FarmeORT.IServicios.Batallas;
using FarmeORT.WebAPI.Filtros;
using Microsoft.AspNetCore.Mvc;

namespace FarmeORT.WebAPI.Controllers;

[ApiController]
[Route("api/v1/batallas/{id:int}/lanzamientos")]
[Autorizacion(Roles.Jugador, Roles.Organizador, Roles.Administrador)]
public sealed class HistorialLanzamientosController(IServicioBatallas servicioBatallas) : ControllerBase
{
    [HttpGet]
    public IActionResult Listar(int id)
    {
        var sesion = HttpContext.ObtenerSesionAutenticada();
        return Ok(servicioBatallas.ListarLanzamientos(id, sesion.UsuarioId, sesion.Rol));
    }
}
