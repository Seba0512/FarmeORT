using System.Text.Json;
using FarmeORT.DTOs.Errores;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace FarmeORT.WebAPI.Validaciones;

internal static class RespuestaValidacion
{
    public static IActionResult Crear(ActionContext contexto)
    {
        var error = new ErrorResponse
        {
            UserMessage = "Los datos enviados no son válidos.",
            ErrorCode = "VALIDATION_ERROR",
            Detalles = ArmarDetalles(contexto.ModelState),
        };
        return new BadRequestObjectResult(error);
    }

    private static List<string> ArmarDetalles(ModelStateDictionary estadoDelModelo)
    {
        var errores = estadoDelModelo
            .SelectMany(campo => campo.Value!.Errors.Select(error => new ErrorDeCampo(campo.Key, error.ErrorMessage)))
            .ToList();

        // Sin cuerpo (clave vacía) o con un JSON que no se puede leer (clave "$..."), el resto de los errores es
        // consecuencia de eso: se informa solo el problema del cuerpo, sin el mensaje técnico del parser.
        var delCuerpo = errores.Where(e => e.Campo.Length == 0 || e.Campo.StartsWith('$')).ToList();
        if(delCuerpo.Count > 0)
        {
            return delCuerpo.Select(e => DescribirErrorDelCuerpo(e.Campo)).Distinct().ToList();
        }

        // ModelState trae el nombre de la propiedad (FechaNacimiento); el contrato usa el del JSON (fechaNacimiento).
        return errores.Select(e => $"{JsonNamingPolicy.CamelCase.ConvertName(e.Campo)}: {e.Mensaje}").ToList();
    }

    private static string DescribirErrorDelCuerpo(string campo)
    {
        if(campo.Length == 0)
        {
            return "El cuerpo de la solicitud es obligatorio.";
        }

        if(campo == "$")
        {
            return "El cuerpo de la solicitud no es un JSON válido.";
        }

        // "$.cupo": el JSON se leyó, pero ese dato no tiene el tipo esperado.
        return $"{campo[2..]}: El valor no tiene un formato válido.";
    }

    private sealed record ErrorDeCampo(string Campo, string Mensaje);
}
