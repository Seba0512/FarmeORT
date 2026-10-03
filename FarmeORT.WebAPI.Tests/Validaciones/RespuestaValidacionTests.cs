using FarmeORT.DTOs.Errores;
using FarmeORT.WebAPI.Validaciones;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;

namespace FarmeORT.WebAPI.Tests.Validaciones;

[TestClass]
public sealed class RespuestaValidacionTests
{
    // Los campos van en camelCase, como en el JSON del contrato.
    [TestMethod]
    public void Crear_ModeloInvalido_Devuelve400ConErrorValidationError()
    {
        var estadoDelModelo = new ModelStateDictionary();
        estadoDelModelo.AddModelError("Email", "El email no tiene un formato válido.");
        var contexto = new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor(), estadoDelModelo);

        var resultado = RespuestaValidacion.Crear(contexto) as BadRequestObjectResult;

        Assert.IsNotNull(resultado);
        Assert.AreEqual(StatusCodes.Status400BadRequest, resultado.StatusCode);
        var error = resultado.Value as ErrorResponse;
        Assert.IsNotNull(error);
        Assert.AreEqual("VALIDATION_ERROR", error.ErrorCode);
        Assert.AreEqual("Los datos enviados no son válidos.", error.UserMessage);
        Assert.IsNotNull(error.Detalles);
        Assert.Contains("email: El email no tiene un formato válido.", error.Detalles);
    }

    // Sin cuerpo o con un JSON que no se puede leer, el "request is required" es consecuencia: se informa solo el
    // problema del cuerpo, sin el mensaje técnico del parser.
    [TestMethod]
    [DataRow("", "A non-empty request body is required.", "El cuerpo de la solicitud es obligatorio.")]
    [DataRow("$", "'a' is an invalid start of a value. Path: $ | LineNumber: 0 | BytePositionInLine: 0.", "El cuerpo de la solicitud no es un JSON válido.")]
    [DataRow("$.cupo", "The JSON value could not be converted to System.Nullable`1[System.Int32]. Path: $.cupo.", "cupo: El valor no tiene un formato válido.")]
    public void Crear_CuerpoQueNoSePuedeLeer_DevuelveSoloElProblemaDelCuerpo(string campo, string mensajeTecnico, string detalle)
    {
        var estadoDelModelo = new ModelStateDictionary();
        estadoDelModelo.AddModelError(campo, mensajeTecnico);
        estadoDelModelo.AddModelError("request", "The request field is required.");
        var contexto = new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor(), estadoDelModelo);

        var resultado = RespuestaValidacion.Crear(contexto) as BadRequestObjectResult;

        var error = resultado?.Value as ErrorResponse;
        Assert.IsNotNull(error);
        Assert.IsNotNull(error.Detalles);
        CollectionAssert.AreEqual(new[] { detalle }, error.Detalles.ToArray());
    }
}
