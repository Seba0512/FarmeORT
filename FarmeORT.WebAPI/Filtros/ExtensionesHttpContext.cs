using FarmeORT.IServicios.Sesiones;

namespace FarmeORT.WebAPI.Filtros;

internal static class ExtensionesHttpContext
{
    // Solo se usa en acciones protegidas por [Autenticacion], que guarda la sesión antes de ejecutarlas.
    public static SesionAutenticada ObtenerSesionAutenticada(this HttpContext httpContext)
    {
        return (SesionAutenticada)httpContext.Items[ClavesContexto.SesionAutenticada]!;
    }

    // En las acciones abiertas a anónimos devuelve null cuando no se envió un token.
    public static SesionAutenticada? ObtenerSesionOpcional(this HttpContext httpContext)
    {
        return httpContext.Items[ClavesContexto.SesionAutenticada] as SesionAutenticada;
    }
}
