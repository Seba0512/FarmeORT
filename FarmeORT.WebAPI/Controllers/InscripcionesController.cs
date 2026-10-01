using FarmeORT.DTOs.Inscripciones;
using FarmeORT.DTOs.Usuarios;
using FarmeORT.IServicios.Inscripciones;
using FarmeORT.IServicios.Sesiones;
using FarmeORT.WebAPI.Filtros;
using Microsoft.AspNetCore.Mvc;

namespace FarmeORT.WebAPI.Controllers;

[ApiController]
[Route("api/v1/inscripciones")]
public sealed class InscripcionesController(IServicioInscripciones servicioInscripciones) : ControllerBase
{
    // El usuario sale siempre del token: nunca de la ruta, la query ni el body.
    private SesionAutenticada Sesion => HttpContext.ObtenerSesionAutenticada();

    [HttpPost]
    [Autorizacion(Roles.Jugador)]
    public IActionResult Inscribir([FromBody] InscripcionRequest request)
    {
        var response = servicioInscripciones.Inscribir(Sesion.UsuarioId, request);
        return Created($"/api/v1/inscripciones/{response.Id}", response);
    }

    [HttpGet]
    [Autenticacion]
    public IActionResult Listar([FromQuery] FiltroInscripcionesRequest filtro)
    {
        return Ok(servicioInscripciones.Listar(filtro, Sesion.UsuarioId, Sesion.Rol));
    }

    [HttpGet("{id:int}")]
    [Autenticacion]
    public IActionResult ObtenerPorId(int id)
    {
        return Ok(servicioInscripciones.ObtenerPorId(id, Sesion.UsuarioId, Sesion.Rol));
    }

    [HttpPost("{id:int}/cancelaciones")]
    [Autorizacion(Roles.Jugador)]
    public IActionResult Cancelar(int id)
    {
        return Ok(servicioInscripciones.Cancelar(id, Sesion.UsuarioId));
    }
}
