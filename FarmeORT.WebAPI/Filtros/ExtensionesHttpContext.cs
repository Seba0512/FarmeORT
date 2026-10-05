using FarmeORT.IServicios.Sesiones;

namespace FarmeORT.WebAPI.Filtros;

internal static class ExtensionesHttpContext
{
    // Solo se usa en acciones protegidas por [Autenticacion], que guarda la sesión antes de ejecutarlas.
    public static SesionAutenticada ObtenerSesionAutenticada(this HttpContext httpContext)
    {
        return (SesionAutenticada)httpContext.Items[ClavesContexto.SesionAutenticada]!;
    }

    // En las acciones abiertas a anónimos ([AutenticacionOpcional]): sin token no se guarda ninguna sesión.
    public static bool EsAnonimo(this HttpContext httpContext)
    {
        return !httpContext.Items.ContainsKey(ClavesContexto.SesionAutenticada);
    }
}
