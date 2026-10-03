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
}
