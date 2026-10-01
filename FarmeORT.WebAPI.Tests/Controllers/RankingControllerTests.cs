using FarmeORT.DTOs.Ranking;
using FarmeORT.IServicios.Ranking;
using FarmeORT.WebAPI.Controllers;
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
        _controller = new RankingController(_servicioRanking.Object);
    }

    [TestMethod]
    public void Obtener_ConCantidad_Devuelve200ConElRanking()
    {
        var filtro = new FiltroRankingRequest { Cantidad = 5 };
        IReadOnlyList<JugadorRankingResponse> ranking = [new JugadorRankingResponse { Posicion = 1, Nickname = "aurafarmer", Elo = 1500 }];
        _servicioRanking.Setup(s => s.Obtener(filtro)).Returns(ranking);

        var resultado = _controller.Obtener(filtro) as OkObjectResult;

        Assert.IsNotNull(resultado);
        Assert.AreEqual(StatusCodes.Status200OK, resultado.StatusCode);
        Assert.AreSame(ranking, resultado.Value);
    }
}
