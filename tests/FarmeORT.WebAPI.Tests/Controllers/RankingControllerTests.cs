using System.Reflection;
using FarmeORT.DTOs.Ranking;
using FarmeORT.DTOs.Usuarios;
using FarmeORT.IServicios.Ranking;
using FarmeORT.IServicios.Sesiones;
using FarmeORT.WebAPI.Controllers;
using FarmeORT.WebAPI.Filtros;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FarmeORT.WebAPI.Tests.Controllers;

[TestClass]
public sealed class RankingControllerTests
{
    private Mock<IServicioRanking> _servicioRanking = null!;
    private RankingController _controller = null!;

    [TestInitialize]
    public void Inicializar()
    {
        _servicioRanking = new Mock<IServicioRanking>(MockBehavior.Strict);
        _controller = new RankingController(_servicioRanking.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() },
        };
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void Obtener_ConCantidad_Devuelve200IndicandoSiConsultaUnAnonimo(bool autenticado)
    {
        AutenticarSi(autenticado);
        var filtro = new FiltroRankingRequest { Cantidad = 5 };
        IReadOnlyList<JugadorRankingResponse> ranking = [new JugadorRankingResponse { Posicion = 1, Nickname = "aurafarmer", Elo = 1500 }];
        _servicioRanking.Setup(s => s.Obtener(filtro, !autenticado)).Returns(ranking);

        var resultado = _controller.Obtener(filtro) as OkObjectResult;

        Assert.IsNotNull(resultado);
        Assert.AreEqual(StatusCodes.Status200OK, resultado.StatusCode);
        Assert.AreSame(ranking, resultado.Value);
    }

    // Público, pero si llega un token se valida: el autenticado ve también los torneos cancelados (foro, debate 11).
    [TestMethod]
    public void Obtener_ContratoDeSeguridad_ExponeGetRankingConAutenticacionOpcional()
    {
        var tipo = typeof(RankingController);
        var metodo = tipo.GetMethod(nameof(RankingController.Obtener))!;

        var atributo = metodo.GetCustomAttribute<AutenticacionAttribute>();

        Assert.AreEqual("api/v1/rankings", tipo.GetCustomAttribute<RouteAttribute>()!.Template);
        Assert.IsNotNull(metodo.GetCustomAttribute<HttpGetAttribute>());
        Assert.IsNull(tipo.GetCustomAttribute<AutenticacionAttribute>());
        Assert.IsInstanceOfType<AutenticacionOpcionalAttribute>(atributo);
    }

    // Sin sesión en el contexto, el controller trata la consulta como anónima.
    private void AutenticarSi(bool autenticado)
    {
        if(autenticado)
        {
            var sesion = new SesionAutenticada { Token = Guid.NewGuid(), UsuarioId = 7, Rol = Roles.Jugador };
            _controller.HttpContext.Items[ClavesContexto.SesionAutenticada] = sesion;
        }
    }
}
