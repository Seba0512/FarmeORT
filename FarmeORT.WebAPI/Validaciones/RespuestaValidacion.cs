using System.Text.Json;
using FarmeORT.DTOs.Errores;
using Microsoft.AspNetCore.Mvc;

namespace FarmeORT.WebAPI.Validaciones;

internal static class RespuestaValidacion
{
    public static IActionResult Crear(ActionContext contexto)
    {
        // ModelState trae el nombre de la propiedad (FechaNacimiento); el contrato usa el del JSON (fechaNacimiento).
        var detalles = contexto.ModelState
            .SelectMany(campo => campo.Value!.Errors.Select(error => $"{JsonNamingPolicy.CamelCase.ConvertName(campo.Key)}: {error.ErrorMessage}"))
            .ToList();
        var error = new ErrorResponse
        {
            UserMessage = "Los datos enviados no son válidos.",
            ErrorCode = "VALIDATION_ERROR",
            Detalles = detalles,
        };
        return new BadRequestObjectResult(error);
    }
}
