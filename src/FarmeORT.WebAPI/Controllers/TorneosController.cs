using FarmeORT.DTOs.Torneos;
using FarmeORT.DTOs.Usuarios;
using FarmeORT.IServicios.Torneos;
using FarmeORT.WebAPI.Filtros;
using Microsoft.AspNetCore.Mvc;

namespace FarmeORT.WebAPI.Controllers;

[ApiController]
[Route("api/v1/torneos")]
public sealed class TorneosController(IServicioTorneos servicioTorneos) : ControllerBase
{
    [HttpPost]
    [Autorizacion(Roles.Administrador)]
    public IActionResult Crear([FromBody] TorneoRequest request)
    {
        var response = servicioTorneos.Crear(request);
        return Created($"/api/v1/torneos/{response.Id}", response);
    }

    [HttpGet]
    [AutenticacionOpcional]
    public IActionResult Listar([FromQuery] FiltroTorneosRequest filtro)
    {
        var esAnonimo = HttpContext.EsAnonimo();
        return Ok(servicioTorneos.Listar(filtro, esAnonimo));
    }

    [HttpGet("{id:int}")]
    [AutenticacionOpcional]
    public IActionResult ObtenerPorId(int id)
    {
        var esAnonimo = HttpContext.EsAnonimo();
        return Ok(servicioTorneos.ObtenerPorId(id, esAnonimo));
    }

    [HttpPut("{id:int}")]
    [Autorizacion(Roles.Administrador)]
    public IActionResult Modificar(int id, [FromBody] TorneoRequest request)
    {
        return Ok(servicioTorneos.Modificar(id, request));
    }

    [HttpPost("{id:int}/cancelaciones")]
    [Autorizacion(Roles.Administrador)]
    public IActionResult Cancelar(int id)
    {
        return Ok(servicioTorneos.Cancelar(id));
    }
}
