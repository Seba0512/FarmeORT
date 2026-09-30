using FarmeORT.DTOs.Errores;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FarmeORT.WebAPI.Filtros;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
internal sealed class AutenticacionAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        if(string.IsNullOrEmpty(context.HttpContext.Request.Headers.Authorization))
        {
            var error = new ErrorResponse { UserMessage = "La sesión no es válida o expiró.", ErrorCode = "INVALID_TOKEN" };
            context.Result = new ObjectResult(error) { StatusCode = StatusCodes.Status401Unauthorized };
        }
    }
}
