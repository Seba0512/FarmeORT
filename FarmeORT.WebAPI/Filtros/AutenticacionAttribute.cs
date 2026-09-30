using FarmeORT.DTOs.Errores;
using FarmeORT.IServicios.Sesiones;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FarmeORT.WebAPI.Filtros;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
internal class AutenticacionAttribute : Attribute, IAuthorizationFilter
{
    private const string PrefijoBearer = "Bearer ";

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var sesion = IntentarObtenerToken(context.HttpContext.Request.Headers.Authorization, out var token)
            ? ObtenerSesion(context, token)
            : null;
        if(sesion is null)
        {
            var error = new ErrorResponse { UserMessage = "La sesión no es válida o expiró.", ErrorCode = "INVALID_TOKEN" };
            context.Result = new ObjectResult(error) { StatusCode = StatusCodes.Status401Unauthorized };
            return;
        }

        context.HttpContext.Items[ClavesContexto.SesionAutenticada] = sesion;
        AutorizarSesion(context, sesion);
    }

    protected virtual void AutorizarSesion(AuthorizationFilterContext context, SesionAutenticada sesion)
    {
    }

    private static bool IntentarObtenerToken(string? encabezado, out Guid token)
    {
        token = Guid.Empty;
        return encabezado is not null
            && encabezado.StartsWith(PrefijoBearer, StringComparison.OrdinalIgnoreCase)
            && Guid.TryParse(encabezado[PrefijoBearer.Length..], out token);
    }

    private static SesionAutenticada? ObtenerSesion(AuthorizationFilterContext context, Guid token)
    {
        var servicioSesiones = context.HttpContext.RequestServices.GetRequiredService<IServicioSesiones>();
        return servicioSesiones.ObtenerSesionAutenticada(token);
    }
}
