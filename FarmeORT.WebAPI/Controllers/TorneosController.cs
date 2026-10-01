using FarmeORT.DTOs.Torneos;
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
    public IActionResult Modificar(int id, [FromBody] TorneoRequest request)
    {
        return Ok(servicioTorneos.Modificar(id, request));
    }

    [HttpPost("{id:int}/cancelaciones")]
    public IActionResult Cancelar(int id)
    {
        _ = id;
        throw new NotImplementedException();
    }
}
