using FarmeORT.DTOs.Errores;
using FarmeORT.IServicios.Sesiones;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FarmeORT.WebAPI.Filtros;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
internal class AutenticacionAttribute : Attribute, IAuthorizationFilter
{
    private const string PrefijoBearer = "Bearer ";

    // Las acciones abiertas a anónimos validan el token solo si el cliente lo envía.
    protected virtual bool PermiteAnonimo => false;

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var encabezado = context.HttpContext.Request.Headers.Authorization;
        if(PermiteAnonimo && string.IsNullOrEmpty(encabezado))
        {
            return;
        }

        var sesion = ObtenerSesion(context, encabezado);
        if(sesion == null)
        {
            var error = new ErrorResponse { UserMessage = "La sesión no es válida.", ErrorCode = "INVALID_TOKEN" };
            context.Result = new ObjectResult(error) { StatusCode = StatusCodes.Status401Unauthorized };
            return;
        }

        context.HttpContext.Items[ClavesContexto.SesionAutenticada] = sesion;
        AutorizarSesion(context, sesion);
    }

    protected virtual void AutorizarSesion(AuthorizationFilterContext context, SesionAutenticada sesion)
    {
    }

    // Sin el formato "Bearer {token}" no se consulta la base: la sesión no es válida.
    private static SesionAutenticada? ObtenerSesion(AuthorizationFilterContext context, string? encabezado)
    {
        if(encabezado == null || !encabezado.StartsWith(PrefijoBearer, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var textoDelToken = encabezado.Substring(PrefijoBearer.Length);
        if(!Guid.TryParse(textoDelToken, out var token))
        {
            return null;
        }

        var servicioSesiones = context.HttpContext.RequestServices.GetRequiredService<IServicioSesiones>();
        return servicioSesiones.ObtenerSesionAutenticada(token);
    }
}
