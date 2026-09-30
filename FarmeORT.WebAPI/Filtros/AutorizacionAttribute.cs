using FarmeORT.IServicios.Sesiones;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FarmeORT.WebAPI.Filtros;

internal sealed class AutorizacionAttribute(params string[] rolesPermitidos) : AutenticacionAttribute
{
    protected override void AutorizarSesion(AuthorizationFilterContext context, SesionAutenticada sesion)
    {
        _ = rolesPermitidos;
        throw new NotImplementedException();
    }
}
