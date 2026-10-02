using FarmeORT.DTOs.Errores;
using FarmeORT.Excepciones;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FarmeORT.WebAPI.Filtros;

internal sealed class FiltroExcepciones : IExceptionFilter
{
    private static readonly ErrorMapeado ErrorInesperado =
        new(StatusCodes.Status500InternalServerError, "INTERNAL_ERROR", "Ocurrió un error inesperado.");

    private static readonly Dictionary<Type, ErrorMapeado> Errores = new()
    {
        [typeof(DatosInvalidosException)] =
            new(StatusCodes.Status400BadRequest, "INVALID_DATA"),
        [typeof(CredencialesInvalidasException)] =
            new(StatusCodes.Status401Unauthorized, "INVALID_CREDENTIALS", "Email o contraseña incorrectos."),
        [typeof(UsuarioNoEncontradoException)] =
            new(StatusCodes.Status404NotFound, "USER_NOT_FOUND", "No existe un usuario con ese id."),
        [typeof(SkillNoEncontradaException)] =
            new(StatusCodes.Status404NotFound, "SKILL_NOT_FOUND", "No existe una skill con ese id."),
        [typeof(SkillNoDeclaradaException)] =
            new(StatusCodes.Status404NotFound, "DECLARED_SKILL_NOT_FOUND", "La skill no está en tu repertorio."),
        [typeof(TorneoNoEncontradoException)] =
            new(StatusCodes.Status404NotFound, "TOURNAMENT_NOT_FOUND", "No existe un torneo con ese id."),
        [typeof(EmailYaExisteException)] =
            new(StatusCodes.Status409Conflict, "EMAIL_ALREADY_EXISTS", "Ya existe un usuario con ese email."),
        [typeof(NicknameYaExisteException)] =
            new(StatusCodes.Status409Conflict, "NICKNAME_ALREADY_EXISTS", "Ya existe un usuario con ese nickname."),
        [typeof(CodigoSkillYaExisteException)] =
            new(StatusCodes.Status409Conflict, "SKILL_CODE_ALREADY_EXISTS", "Ya existe una skill con ese código."),
        [typeof(SkillInactivaException)] =
            new(StatusCodes.Status409Conflict, "SKILL_INACTIVE", "No es posible utilizar la skill porque está inactiva."),
        [typeof(SkillYaDeclaradaException)] =
            new(StatusCodes.Status409Conflict, "SKILL_ALREADY_DECLARED", "La skill ya está en tu repertorio."),
        [typeof(CodigoTorneoYaExisteException)] =
            new(StatusCodes.Status409Conflict, "TOURNAMENT_CODE_ALREADY_EXISTS", "Ya existe un torneo con ese código."),
        [typeof(TorneoNoModificableException)] =
            new(StatusCodes.Status409Conflict, "TOURNAMENT_NOT_MODIFIABLE", "Solo se puede modificar un torneo Abierto."),
        [typeof(TorneoNoCancelableException)] =
            new(StatusCodes.Status409Conflict, "TOURNAMENT_NOT_CANCELLABLE", "Solo se puede cancelar un torneo Abierto o En curso."),
        [typeof(OperacionSobreSiMismoException)] =
            new(StatusCodes.Status409Conflict, "SELF_OPERATION_NOT_ALLOWED", "Un usuario no puede modificarse ni eliminarse a sí mismo."),

        // Skills obligatorias del torneo (D-40).
        [typeof(NoEsOrganizadorDelTorneoException)] =
            new(StatusCodes.Status403Forbidden, "NOT_TOURNAMENT_ORGANIZER", "No sos el organizador asignado a este torneo."),
        [typeof(SkillObligatoriaYaAsociadaException)] =
            new(StatusCodes.Status409Conflict, "MANDATORY_SKILL_ALREADY_ADDED", "La skill ya es obligatoria en este torneo."),
        [typeof(LimiteSkillsObligatoriasException)] =
            new(StatusCodes.Status409Conflict, "MANDATORY_SKILLS_LIMIT_REACHED", "Un torneo puede tener como máximo 2 skills obligatorias."),
        [typeof(SkillObligatoriaNoEncontradaException)] =
            new(StatusCodes.Status404NotFound, "MANDATORY_SKILL_NOT_FOUND", "La skill no es obligatoria en este torneo."),

        // Inscripciones (D-41).
        [typeof(SkillsObligatoriasFaltantesException)] =
            new(StatusCodes.Status409Conflict, "MISSING_MANDATORY_SKILLS", "No tenés declaradas todas las skills obligatorias del torneo."),
        [typeof(JugadoresSinSkillObligatoriaException)] =
            new(StatusCodes.Status409Conflict, "PLAYERS_MISSING_MANDATORY_SKILL", "Hay jugadores inscriptos que no tienen declarada la skill."),
        [typeof(InscripcionNoEncontradaException)] =
            new(StatusCodes.Status404NotFound, "REGISTRATION_NOT_FOUND", "No existe una inscripción con ese id."),
        [typeof(NoEsDuenoDeLaInscripcionException)] =
            new(StatusCodes.Status403Forbidden, "NOT_REGISTRATION_OWNER", "La inscripción no es tuya."),
        [typeof(TorneoNoAbiertoException)] =
            new(StatusCodes.Status409Conflict, "TOURNAMENT_NOT_OPEN", "El torneo no está abierto a inscripciones."),
        [typeof(InscripcionYaExisteException)] =
            new(StatusCodes.Status409Conflict, "ALREADY_REGISTERED", "Ya tenés una inscripción aceptada en este torneo."),
        [typeof(TorneoCompletoException)] =
            new(StatusCodes.Status409Conflict, "TOURNAMENT_FULL", "El torneo ya alcanzó su cupo de inscripciones aceptadas."),
        [typeof(InscripcionNoActivaException)] =
            new(StatusCodes.Status409Conflict, "REGISTRATION_NOT_ACTIVE", "La inscripción ya no está aceptada."),
        [typeof(SkillObligatoriaEnUsoException)] =
            new(StatusCodes.Status409Conflict, "MANDATORY_SKILL_IN_USE", "No podés quitar la skill: es obligatoria en un torneo en el que estás inscripto."),

        // Persona B: batallas y lanzamientos.
        [typeof(BatallaNoEncontradaException)] =
            new(StatusCodes.Status404NotFound, "BATTLE_NOT_FOUND", "No existe una batalla con ese id."),
        [typeof(BatallaNoCancelableException)] =
            new(StatusCodes.Status409Conflict, "BATTLE_NOT_CANCELLABLE", "Solo se puede cancelar una batalla Pendiente o En arbitraje."),
        [typeof(BatallaNoPendienteException)] =
            new(StatusCodes.Status409Conflict, "BATTLE_NOT_PENDING", "Solo se puede lanzar una skill en una batalla Pendiente."),
        [typeof(JugadorNoParticipaEnBatallaException)] =
            new(StatusCodes.Status409Conflict, "PLAYER_NOT_IN_BATTLE", "El jugador no participa de esta batalla."),
        [typeof(TurnoIncorrectoException)] =
            new(StatusCodes.Status409Conflict, "NOT_PLAYER_TURN", "No es tu turno para lanzar una skill."),
        [typeof(SkillFueraDelRepertorioException)] =
            new(StatusCodes.Status409Conflict, "SKILL_NOT_IN_REPERTOIRE", "La skill no está en tu repertorio."),
        [typeof(SkillYaLanzadaException)] =
            new(StatusCodes.Status409Conflict, "SKILL_ALREADY_THROWN", "Ya lanzaste esta skill en la batalla."),
        [typeof(BatallaModificadaException)] =
            new(StatusCodes.Status409Conflict, "BATTLE_CHANGED", "La batalla cambió mientras se procesaba el pedido. Volvé a consultarla."),
        [typeof(SkillObligatoriaPendienteException)] =
            new(StatusCodes.Status409Conflict, "MANDATORY_SKILL_PENDING", "Debés lanzar una skill obligatoria pendiente para completar tus cuatro turnos."),

        // Persona A: eliminación y cambio de rol de usuarios (D-43).
        [typeof(UsuarioOrganizaTorneoVigenteException)] =
            new(StatusCodes.Status409Conflict, "USER_ORGANIZES_ACTIVE_TOURNAMENT", "El usuario organiza un torneo que no está Finalizado ni Cancelado."),
        [typeof(UsuarioConBatallasSinResolverException)] =
            new(StatusCodes.Status409Conflict, "USER_HAS_UNRESOLVED_BATTLES", "El usuario tiene batallas Pendientes o En arbitraje."),
        [typeof(UsuarioConInscripcionVigenteException)] =
            new(StatusCodes.Status409Conflict, "USER_HAS_ACTIVE_REGISTRATION", "El usuario está inscripto en un torneo que no está Finalizado ni Cancelado."),

        // Persona B: fallo de batalla (D-53).
        [typeof(BatallaNoFallableException)] =
            new(StatusCodes.Status409Conflict, "BATTLE_NOT_JUDGEABLE", "Solo se puede fallar una batalla Pendiente o En arbitraje."),

        // Persona B: visibilidad de consultas de batallas (D-55).
        [typeof(JugadorNoInscriptoEnTorneoException)] =
            new(StatusCodes.Status403Forbidden, "PLAYER_NOT_IN_TOURNAMENT", "No tenés una inscripción en este torneo."),

        // Persona C: rondas (D-62).
        [typeof(TorneoIncompletoException)] =
            new(StatusCodes.Status409Conflict, "TOURNAMENT_NOT_FULL", "El torneo todavía no completó su cupo de inscripciones aceptadas."),
        [typeof(RondaNoEncontradaException)] =
            new(StatusCodes.Status404NotFound, "ROUND_NOT_FOUND", "El torneo no tiene una ronda con ese número."),
        [typeof(RondaSinResolverException)] =
            new(StatusCodes.Status409Conflict, "ROUND_NOT_RESOLVED", "La ronda anterior todavía tiene batallas sin resolver."),
        [typeof(TorneoListoParaFinalizarException)] =
            new(StatusCodes.Status409Conflict, "TOURNAMENT_READY_TO_FINISH", "Queda un solo jugador sin eliminar: corresponde finalizar el torneo."),
        [typeof(TorneoNoEnCursoException)] =
            new(StatusCodes.Status409Conflict, "TOURNAMENT_NOT_IN_PROGRESS", "El torneo no está en curso."),
        [typeof(TorneoConBatallasSinResolverException)] =
            new(StatusCodes.Status409Conflict, "TOURNAMENT_HAS_UNRESOLVED_BATTLES", "El torneo todavía tiene batallas sin resolver."),
    };

    public void OnException(ExceptionContext context)
    {
        var errorMapeado = Errores.GetValueOrDefault(context.Exception.GetType(), ErrorInesperado);
        var error = new ErrorResponse
        {
            UserMessage = errorMapeado.UserMessage ?? context.Exception.Message,
            ErrorCode = errorMapeado.ErrorCode,
            Detalles = (context.Exception as ExcepcionConDetalles)?.Detalles,
        };
        context.Result = new ObjectResult(error) { StatusCode = errorMapeado.Status };
    }

    // Sin UserMessage, se muestra el mensaje de la excepción: solo para reglas del dominio, que no exponen datos internos.
    private sealed record ErrorMapeado(int Status, string ErrorCode, string? UserMessage = null);
}
