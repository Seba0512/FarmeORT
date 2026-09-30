using FarmeORT.DTOs.Usuarios;
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

    [HttpGet("{id:int}")]
    public IActionResult ObtenerPorId(int id)
    {
        return Ok(servicioUsuarios.ObtenerPorId(id));
    }
}
