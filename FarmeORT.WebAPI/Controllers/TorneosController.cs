using FarmeORT.DTOs.Torneos;
using FarmeORT.IServicios.Torneos;
using FarmeORT.WebAPI.Filtros;
using Microsoft.AspNetCore.Mvc;

namespace FarmeORT.WebAPI.Controllers;

[ApiController]
[Route("api/v1/torneos")]
public sealed class TorneosController(IServicioTorneos servicioTorneos) : ControllerBase
{
    [HttpPost]
    public IActionResult Crear([FromBody] TorneoRequest request)
    {
        var response = servicioTorneos.Crear(request);
        return Created($"/api/v1/torneos/{response.Id}", response);
    }

    [HttpGet]
    public IActionResult Listar([FromQuery] FiltroTorneosRequest filtro)
    {
        _ = filtro;
        throw new NotImplementedException();
    }

    [HttpGet("{id:int}")]
    public IActionResult ObtenerPorId(int id)
    {
        var esAnonimo = HttpContext.ObtenerSesionOpcional() is null;
        return Ok(servicioTorneos.ObtenerPorId(id, esAnonimo));
    }
}
