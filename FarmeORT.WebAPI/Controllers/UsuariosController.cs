using FarmeORT.DTOs.Usuarios;
using FarmeORT.IServicios.Usuarios;
using Microsoft.AspNetCore.Mvc;

namespace FarmeORT.WebAPI.Controllers;

[ApiController]
[Route("api/v1/usuarios")]
public sealed class UsuariosController(IServicioUsuarios servicioUsuarios) : ControllerBase
{
    [HttpPost]
    public IActionResult Crear([FromBody] UsuarioAdminRequest request)
    {
        _ = servicioUsuarios;
        _ = request;
        throw new NotImplementedException();
    }
}
