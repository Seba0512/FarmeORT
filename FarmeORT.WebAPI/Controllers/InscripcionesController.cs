using FarmeORT.DTOs.Inscripciones;
using FarmeORT.IServicios.Inscripciones;
using FarmeORT.WebAPI.Filtros;
using Microsoft.AspNetCore.Mvc;

namespace FarmeORT.WebAPI.Controllers;

[ApiController]
[Route("api/v1/inscripciones")]
public sealed class InscripcionesController(IServicioInscripciones servicioInscripciones) : ControllerBase
{
    [HttpPost]
    public IActionResult Inscribir([FromBody] InscripcionRequest request)
    {
        var sesion = HttpContext.ObtenerSesionAutenticada();
        var response = servicioInscripciones.Inscribir(sesion.UsuarioId, request);
        return Created($"/api/v1/inscripciones/{response.Id}", response);
    }

    [HttpGet]
    public IActionResult Listar([FromQuery] FiltroInscripcionesRequest filtro)
    {
        _ = filtro;
        throw new NotImplementedException();
    }
}
