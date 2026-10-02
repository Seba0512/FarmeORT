using FarmeORT.DTOs.Usuarios;
using FarmeORT.IServicios.Rondas;
using FarmeORT.WebAPI.Filtros;
using Microsoft.AspNetCore.Mvc;

namespace FarmeORT.WebAPI.Controllers;

[ApiController]
[Route("api/v1/torneos/{torneoId:int}/rondas")]
public sealed class RondasController(IServicioRondas servicioRondas) : ControllerBase
{
    [HttpPost]
    [Autorizacion(Roles.Organizador, Roles.Administrador)]
    public IActionResult Generar(int torneoId)
    {
        var sesion = HttpContext.ObtenerSesionAutenticada();
        var response = servicioRondas.Generar(torneoId, sesion.UsuarioId, sesion.Rol);
        return Created($"/api/v1/torneos/{torneoId}/rondas/{response.Numero}", response);
    }

    [HttpGet]
    public IActionResult Listar(int torneoId)
    {
        var esAnonimo = HttpContext.ObtenerSesionOpcional() is null;
        return Ok(servicioRondas.Listar(torneoId, esAnonimo));
    }

    [HttpGet("{numero:int}")]
    public IActionResult ObtenerPorNumero(int torneoId, int numero)
    {
        var esAnonimo = HttpContext.ObtenerSesionOpcional() is null;
        return Ok(servicioRondas.ObtenerPorNumero(torneoId, numero, esAnonimo));
    }
}
