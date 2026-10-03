using FarmeORT.Dominio.Skills;
using FarmeORT.Dominio.Torneos;
using FarmeORT.DTOs.Skills;
using FarmeORT.DTOs.Torneos;
using FarmeORT.Excepciones;
using FarmeORT.IRepositorios.Inscripciones;
using FarmeORT.IRepositorios.Skills;
using FarmeORT.IRepositorios.Torneos;
using FarmeORT.IServicios.Torneos;
using FarmeORT.Servicios.Skills;

namespace FarmeORT.Servicios.Torneos;

internal sealed class ServicioSkillsObligatorias(
    IRepositorioTorneos repositorioTorneos,
    IRepositorioSkills repositorioSkills,
    IRepositorioInscripciones repositorioInscripciones) : IServicioSkillsObligatorias
{
    public IReadOnlyList<SkillResponse> Listar(int torneoId, bool esAnonimo)
    {
        var torneo = ObtenerTorneoExistente(torneoId);
        if(esAnonimo && !torneo.EsVisibleParaAnonimos)
        {
            throw new TorneoNoEncontradoException();
        }

        return torneo.SkillsObligatorias
            .OrderBy(s => s.Id)
            .Select(RespuestasSkills.Crear)
            .ToList();
    }

    public SkillResponse Asociar(int torneoId, SkillObligatoriaRequest request, int organizadorId)
    {
        var skillId = DatosObligatorios.Obtener(request.SkillId, "La skill es obligatoria.");
        var torneo = ObtenerTorneoDelOrganizador(torneoId, organizadorId);
        var skill = repositorioSkills.ObtenerPorId(skillId);
        if(skill == null)
        {
            throw new SkillNoEncontradaException();
        }

        // Primero las reglas del torneo y de la skill: una skill inactiva se rechaza con su propio mensaje (letra 2.2).
        torneo.ValidarSkillObligatoriaNueva(skill);
        ValidarQueLaTenganLosInscriptos(torneoId, skill);
        torneo.AsociarSkillObligatoria(skill);
        repositorioTorneos.Actualizar(torneo);
        return RespuestasSkills.Crear(skill);
    }

    public void Quitar(int torneoId, int skillId, int organizadorId)
    {
        var torneo = ObtenerTorneoDelOrganizador(torneoId, organizadorId);
        torneo.QuitarSkillObligatoria(skillId);
        repositorioTorneos.Actualizar(torneo);
    }

    // Letra 2.4: todos los inscriptos Aceptados tienen que poder cumplir la nueva obligatoria.
    private void ValidarQueLaTenganLosInscriptos(int torneoId, Skill skill)
    {
        var sinLaSkill = repositorioInscripciones.ListarAceptadasDelTorneo(torneoId)
            .Where(i => i.Jugador.SkillsFaltantes([skill]).Count > 0)
            .Select(i => i.Jugador.Nickname)
            .ToList();
        if(sinLaSkill.Count > 0)
        {
            throw new JugadoresSinSkillObligatoriaException(sinLaSkill);
        }
    }

    private Torneo ObtenerTorneoExistente(int torneoId)
    {
        var torneo = repositorioTorneos.ObtenerPorId(torneoId);
        if(torneo == null)
        {
            throw new TorneoNoEncontradoException();
        }

        return torneo;
    }

    // El filtro solo sabe que es un Organizador; si es el asignado a este torneo lo decide el servicio (403, D-40).
    private Torneo ObtenerTorneoDelOrganizador(int torneoId, int organizadorId)
    {
        var torneo = ObtenerTorneoExistente(torneoId);
        if(torneo.OrganizadorId != organizadorId)
        {
            throw new NoEsOrganizadorDelTorneoException();
        }

        return torneo;
    }
}
