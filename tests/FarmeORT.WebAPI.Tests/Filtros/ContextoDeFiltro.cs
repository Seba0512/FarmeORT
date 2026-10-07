using FarmeORT.IServicios.Sesiones;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace FarmeORT.WebAPI.Tests.Filtros;

internal static class ContextoDeFiltro
{
    public static AuthorizationFilterContext Crear(IServicioSesiones servicioSesiones, string? encabezadoAuthorization = null)
    {
        var servicios = new ServiceCollection().AddSingleton(servicioSesiones).BuildServiceProvider();
        var contextoHttp = new DefaultHttpContext { RequestServices = servicios };
        if(encabezadoAuthorization is not null)
        {
            contextoHttp.Request.Headers.Authorization = encabezadoAuthorization;
        }

        var contextoAccion = new ActionContext(contextoHttp, new RouteData(), new ActionDescriptor());
        return new AuthorizationFilterContext(contextoAccion, []);
    }
}
