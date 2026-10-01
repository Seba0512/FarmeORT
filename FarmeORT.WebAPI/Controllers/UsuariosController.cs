using FarmeORT.DTOs.Usuarios;
using FarmeORT.IServicios.Sesiones;
using FarmeORT.IServicios.Usuarios;
using FarmeORT.WebAPI.Filtros;
using Microsoft.AspNetCore.Mvc;

namespace FarmeORT.WebAPI.Controllers;

[ApiController]
[Route("api/v1/usuarios")]
[Autorizacion(Roles.Administrador)]
public sealed class UsuariosController(IServicioUsuarios servicioUsuarios) : ControllerBase
{
    [HttpPost]
    public IActionResult Crear([FromBody] UsuarioAdminRequest request)
    {
        var response = servicioUsuarios.Crear(request);
        return Created($"/api/v1/usuarios/{response.Id}", response);
    }

    [HttpGet]
    public IActionResult Listar([FromQuery] FiltroUsuariosRequest filtro)
    {
        return Ok(servicioUsuarios.Listar(filtro));
    }

    [HttpGet("{id:int}")]
    public IActionResult ObtenerPorId(int id)
    {
        return Ok(servicioUsuarios.ObtenerPorId(id));
    }

    [HttpPut("{id:int}")]
    public IActionResult Modificar(int id, [FromBody] UsuarioAdminRequest request)
    {
        var sesion = (SesionAutenticada)HttpContext.Items[ClavesContexto.SesionAutenticada]!;
        return Ok(servicioUsuarios.Modificar(id, request, sesion.UsuarioId));
    }

    [HttpDelete("{id:int}")]
    public IActionResult Eliminar(int id)
    {
        var sesion = (SesionAutenticada)HttpContext.Items[ClavesContexto.SesionAutenticada]!;
        servicioUsuarios.Eliminar(id, sesion.UsuarioId);
        return NoContent();
    }
}
