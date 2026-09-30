using Microsoft.AspNetCore.Mvc.Filters;

namespace FarmeORT.WebAPI.Filtros;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
internal sealed class AutenticacionAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        throw new NotImplementedException();
    }
}
