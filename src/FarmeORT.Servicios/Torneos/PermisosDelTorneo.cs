using FarmeORT.Dominio.Torneos;
using FarmeORT.DTOs.Usuarios;
using FarmeORT.Excepciones;

namespace FarmeORT.Servicios.Torneos;

// El filtro solo sabe el rol: si un Organizador es el asignado a este torneo lo decide el servicio (D-40).
// Generar rondas y finalizar el torneo también los puede hacer un Administrador (D-63).
internal static class PermisosDelTorneo
{
    public static void ValidarOrganizadorOAdministrador(Torneo torneo, int usuarioId, string rol)
    {
        if(rol != Roles.Administrador && torneo.OrganizadorId != usuarioId)
        {
            throw new NoEsOrganizadorDelTorneoException();
        }
    }
}
