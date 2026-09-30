using FarmeORT.DTOs.Errores;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FarmeORT.WebAPI.Filtros;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
internal sealed class AutenticacionAttribute : Attribute, IAuthorizationFilter
{
    private const string PrefijoBearer = "Bearer ";

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        if(!IntentarObtenerToken(context.HttpContext.Request.Headers.Authorization, out _))
        {
            var error = new ErrorResponse { UserMessage = "La sesión no es válida o expiró.", ErrorCode = "INVALID_TOKEN" };
            context.Result = new ObjectResult(error) { StatusCode = StatusCodes.Status401Unauthorized };
        }
    }

    private static bool IntentarObtenerToken(string? encabezado, out Guid token)
    {
        token = Guid.Empty;
        return encabezado is not null
            && encabezado.StartsWith(PrefijoBearer, StringComparison.OrdinalIgnoreCase)
            && Guid.TryParse(encabezado[PrefijoBearer.Length..], out token);
    }
}
