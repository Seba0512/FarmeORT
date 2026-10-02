using FarmeORT.DTOs.Batallas;
using FarmeORT.DTOs.Usuarios;
using FarmeORT.IServicios.Batallas;
using FarmeORT.WebAPI.Filtros;
using Microsoft.AspNetCore.Mvc;

namespace FarmeORT.WebAPI.Controllers;

[ApiController]
[Route("api/v1/batallas")]
[Autorizacion(Roles.Jugador, Roles.Organizador, Roles.Administrador)]
public sealed class BatallasController(IServicioBatallas servicioBatallas) : ControllerBase
{
    [HttpGet]
    public IActionResult Listar([FromQuery] FiltroBatallasRequest filtro)
    {
        var sesion = HttpContext.ObtenerSesionAutenticada();
        return Ok(servicioBatallas.Listar(filtro, sesion.UsuarioId, sesion.Rol));
    }

    [HttpGet("{id:int}")]
    public IActionResult ObtenerPorId(int id)
    {
        var sesion = HttpContext.ObtenerSesionAutenticada();
        return Ok(servicioBatallas.ObtenerPorId(id, sesion.UsuarioId, sesion.Rol));
    }
}
