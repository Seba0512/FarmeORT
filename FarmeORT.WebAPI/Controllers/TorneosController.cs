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
    private bool ConsultaUnAnonimo => HttpContext.ObtenerSesionOpcional() is null;

    [HttpPost]
    [Autorizacion(Roles.Administrador)]
    public IActionResult Crear([FromBody] TorneoRequest request)
    {
        var response = servicioTorneos.Crear(request);
        return Created($"/api/v1/torneos/{response.Id}", response);
    }

    [HttpGet]
    public IActionResult Listar([FromQuery] FiltroTorneosRequest filtro)
    {
        return Ok(servicioTorneos.Listar(filtro, ConsultaUnAnonimo));
    }

    [HttpGet("{id:int}")]
    public IActionResult ObtenerPorId(int id)
    {
        return Ok(servicioTorneos.ObtenerPorId(id, ConsultaUnAnonimo));
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
