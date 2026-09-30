using FarmeORT.DTOs.Errores;
using FarmeORT.IServicios.Sesiones;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FarmeORT.WebAPI.Filtros;

internal sealed class AutorizacionAttribute(params string[] rolesPermitidos) : AutenticacionAttribute
{
    public IReadOnlyCollection<string> RolesPermitidos { get; } = rolesPermitidos;

    protected override void AutorizarSesion(AuthorizationFilterContext context, SesionAutenticada sesion)
    {
        if(!RolesPermitidos.Contains(sesion.Rol))
        {
            var error = new ErrorResponse { UserMessage = "No tenés permisos para realizar esta operación.", ErrorCode = "FORBIDDEN" };
            context.Result = new ObjectResult(error) { StatusCode = StatusCodes.Status403Forbidden };
        }
    }
}
