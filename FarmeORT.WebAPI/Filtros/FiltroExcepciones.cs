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

        // Persona B: batallas y lanzamientos.
        [typeof(BatallaNoEncontradaException)] =
            new(StatusCodes.Status404NotFound, "BATTLE_NOT_FOUND", "No existe una batalla con ese id."),
        [typeof(BatallaNoCancelableException)] =
            new(StatusCodes.Status409Conflict, "BATTLE_NOT_CANCELLABLE", "Solo se puede cancelar una batalla Pendiente o En arbitraje."),
        [typeof(BatallaNoPendienteException)] =
            new(StatusCodes.Status409Conflict, "BATTLE_NOT_PENDING", "Solo se puede lanzar una skill en una batalla Pendiente."),
        [typeof(JugadorNoParticipaEnBatallaException)] =
            new(StatusCodes.Status409Conflict, "PLAYER_NOT_IN_BATTLE", "El jugador no participa de esta batalla."),
    };

    public void OnException(ExceptionContext context)
    {
        var errorMapeado = Errores.GetValueOrDefault(context.Exception.GetType(), ErrorInesperado);
        var error = new ErrorResponse
        {
            UserMessage = errorMapeado.UserMessage ?? context.Exception.Message,
            ErrorCode = errorMapeado.ErrorCode,
        };
        context.Result = new ObjectResult(error) { StatusCode = errorMapeado.Status };
    }

    // Sin UserMessage, se muestra el mensaje de la excepción: solo para reglas del dominio, que no exponen datos internos.
    private sealed record ErrorMapeado(int Status, string ErrorCode, string? UserMessage = null);
}
