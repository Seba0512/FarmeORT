using FarmeORT.DTOs.Ranking;
using FarmeORT.IServicios.Ranking;
using Microsoft.AspNetCore.Mvc;

namespace FarmeORT.WebAPI.Controllers;

[ApiController]
[Route("api/v1/ranking")]
public sealed class RankingController(IServicioRanking servicioRanking) : ControllerBase
{
    [HttpGet]
    public IActionResult Obtener([FromQuery] FiltroRankingRequest filtro)
    {
        _ = servicioRanking;
        _ = filtro;
        throw new NotImplementedException();
    }
}
