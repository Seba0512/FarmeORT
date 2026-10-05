using FarmeORT.DTOs.Batallas;
using FarmeORT.DTOs.Usuarios;
using FarmeORT.IServicios.Batallas;
using FarmeORT.WebAPI.Filtros;
using Microsoft.AspNetCore.Mvc;

namespace FarmeORT.WebAPI.Controllers;

// Un solo recurso con dos permisos: el historial lo consultan los tres roles y lanzar es solo del Jugador (D-55).
[ApiController]
[Route("api/v1/batallas/{id:int}/lanzamientos")]
public sealed class LanzamientosController(
    IServicioLanzamientos servicioLanzamientos,
    IServicioBatallas servicioBatallas) : ControllerBase
{
    [HttpGet]
    [Autorizacion(Roles.Jugador, Roles.Organizador, Roles.Administrador)]
    public IActionResult Listar(int id)
    {
        var sesion = HttpContext.ObtenerSesionAutenticada();
        return Ok(servicioBatallas.ListarLanzamientos(id, sesion.UsuarioId, sesion.Rol));
    }

    [HttpPost]
    [Autorizacion(Roles.Jugador)]
    public IActionResult Lanzar(int id, [FromBody] LanzamientoRequest request)
    {
        var jugadorId = HttpContext.ObtenerSesionAutenticada().UsuarioId;
        var response = servicioLanzamientos.Lanzar(id, jugadorId, request);
        return Created($"/api/v1/batallas/{id}/lanzamientos", response);
    }
}
