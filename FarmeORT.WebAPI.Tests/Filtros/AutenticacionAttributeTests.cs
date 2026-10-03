using FarmeORT.DTOs.Errores;
using FarmeORT.DTOs.Usuarios;
using FarmeORT.IServicios.Sesiones;
using FarmeORT.WebAPI.Filtros;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FarmeORT.WebAPI.Tests.Filtros;

[TestClass]
public sealed class AutenticacionAttributeTests
{
    private static readonly Guid Token = Guid.Parse("3f2b8c1e-5d4a-4e7b-9c6f-1a2b3c4d5e6f");

    private Mock<IServicioSesiones> _servicioSesiones = null!;
    private AutenticacionAttribute _filtro = null!;

    [TestInitialize]
    public void Inicializar()
    {
        _servicioSesiones = new Mock<IServicioSesiones>(MockBehavior.Strict);
        _filtro = new AutenticacionAttribute();
    }

    // Las sesiones no vencen (la letra no lo pide): el mensaje no puede decir que expiró.
    [TestMethod]
    public void OnAuthorization_SinHeaderAuthorization_Devuelve401ConInvalidToken()
    {
        var contexto = ContextoDeFiltro.Crear(_servicioSesiones.Object);

        _filtro.OnAuthorization(contexto);

        var resultado = contexto.Result as ObjectResult;
        Assert.IsNotNull(resultado);
        Assert.AreEqual(StatusCodes.Status401Unauthorized, resultado.StatusCode);
        var error = resultado.Value as ErrorResponse;
        Assert.IsNotNull(error);
        Assert.AreEqual("INVALID_TOKEN", error.ErrorCode);
        Assert.AreEqual("La sesión no es válida.", error.UserMessage);
    }

    [TestMethod]
    [DataRow("Basic dXN1YXJpbzpjbGF2ZQ==")]
    [DataRow("Bearer")]
    [DataRow("Bearer no-es-un-guid")]
    public void OnAuthorization_HeaderConFormatoInvalido_Devuelve401ConInvalidToken(string encabezado)
    {
        var contexto = ContextoDeFiltro.Crear(_servicioSesiones.Object, encabezado);

        _filtro.OnAuthorization(contexto);

        var resultado = contexto.Result as ObjectResult;
        Assert.IsNotNull(resultado);
        Assert.AreEqual(StatusCodes.Status401Unauthorized, resultado.StatusCode);
        var error = resultado.Value as ErrorResponse;
        Assert.IsNotNull(error);
        Assert.AreEqual("INVALID_TOKEN", error.ErrorCode);
    }

    [TestMethod]
    public void OnAuthorization_TokenInexistente_Devuelve401ConInvalidToken()
    {
        var contexto = ContextoDeFiltro.Crear(_servicioSesiones.Object, $"Bearer {Token}");
        _servicioSesiones.Setup(s => s.ObtenerSesionAutenticada(Token)).Returns((SesionAutenticada?)null);

        _filtro.OnAuthorization(contexto);

        var resultado = contexto.Result as ObjectResult;
        Assert.IsNotNull(resultado);
        Assert.AreEqual(StatusCodes.Status401Unauthorized, resultado.StatusCode);
        var error = resultado.Value as ErrorResponse;
        Assert.IsNotNull(error);
        Assert.AreEqual("INVALID_TOKEN", error.ErrorCode);
    }

    [TestMethod]
    public void OnAuthorization_TokenValido_GuardaLaSesionYNoCortaElRequest()
    {
        var contexto = ContextoDeFiltro.Crear(_servicioSesiones.Object, $"Bearer {Token}");
        var sesion = new SesionAutenticada { Token = Token, UsuarioId = 7, Rol = Roles.Jugador };
        _servicioSesiones.Setup(s => s.ObtenerSesionAutenticada(Token)).Returns(sesion);

        _filtro.OnAuthorization(contexto);

        Assert.IsNull(contexto.Result);
        Assert.AreSame(sesion, contexto.HttpContext.Items[ClavesContexto.SesionAutenticada]);
    }
}
