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
        [typeof(EmailYaExisteException)] =
            new(StatusCodes.Status409Conflict, "EMAIL_ALREADY_EXISTS", "Ya existe un usuario con ese email."),
        [typeof(NicknameYaExisteException)] =
            new(StatusCodes.Status409Conflict, "NICKNAME_ALREADY_EXISTS", "Ya existe un usuario con ese nickname."),
        [typeof(CodigoSkillYaExisteException)] =
            new(StatusCodes.Status409Conflict, "SKILL_CODE_ALREADY_EXISTS", "Ya existe una skill con ese código."),
        [typeof(OperacionSobreSiMismoException)] =
            new(StatusCodes.Status409Conflict, "SELF_OPERATION_NOT_ALLOWED", "Un usuario no puede modificarse ni eliminarse a sí mismo."),
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
