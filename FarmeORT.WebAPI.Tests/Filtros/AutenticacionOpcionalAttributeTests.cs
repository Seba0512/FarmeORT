using FarmeORT.DTOs.Errores;
using FarmeORT.DTOs.Usuarios;
using FarmeORT.IServicios.Sesiones;
using FarmeORT.WebAPI.Filtros;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FarmeORT.WebAPI.Tests.Filtros;

[TestClass]
public sealed class AutenticacionOpcionalAttributeTests
{
    private static readonly Guid Token = Guid.Parse("3f2b8c1e-5d4a-4e7b-9c6f-1a2b3c4d5e6f");

    private Mock<IServicioSesiones> _servicioSesiones = null!;
    private AutenticacionOpcionalAttribute _filtro = null!;

    [TestInitialize]
    public void Inicializar()
    {
        _servicioSesiones = new Mock<IServicioSesiones>(MockBehavior.Strict);
        _filtro = new AutenticacionOpcionalAttribute();
    }

    [TestMethod]
    public void OnAuthorization_SinHeaderAuthorization_SigueComoAnonimoSinGuardarSesion()
    {
        var contexto = ContextoDeFiltro.Crear(_servicioSesiones.Object);

        _filtro.OnAuthorization(contexto);

        Assert.IsNull(contexto.Result);
        Assert.IsFalse(contexto.HttpContext.Items.ContainsKey(ClavesContexto.SesionAutenticada));
    }

    [TestMethod]
    [DataRow("Bearer no-es-un-guid")]
    [DataRow("Bearer 3f2b8c1e-5d4a-4e7b-9c6f-1a2b3c4d5e6f")]
    public void OnAuthorization_TokenEnviadoPeroInvalido_Devuelve401ConInvalidToken(string encabezado)
    {
        var contexto = ContextoDeFiltro.Crear(_servicioSesiones.Object, encabezado);
        _servicioSesiones.Setup(s => s.ObtenerSesionAutenticada(Token)).Returns((SesionAutenticada?)null);

        _filtro.OnAuthorization(contexto);

        var resultado = contexto.Result as ObjectResult;
        Assert.IsNotNull(resultado);
        Assert.AreEqual(StatusCodes.Status401Unauthorized, resultado.StatusCode);
        Assert.AreEqual("INVALID_TOKEN", ((ErrorResponse)resultado.Value!).ErrorCode);
    }

    [TestMethod]
    public void OnAuthorization_TokenValido_GuardaLaSesion()
    {
        var sesion = new SesionAutenticada { Token = Token, UsuarioId = 7, Rol = Roles.Jugador };
        var contexto = ContextoDeFiltro.Crear(_servicioSesiones.Object, $"Bearer {Token}");
        _servicioSesiones.Setup(s => s.ObtenerSesionAutenticada(Token)).Returns(sesion);

        _filtro.OnAuthorization(contexto);

        Assert.IsNull(contexto.Result);
        Assert.AreSame(sesion, contexto.HttpContext.Items[ClavesContexto.SesionAutenticada]);
    }
}
