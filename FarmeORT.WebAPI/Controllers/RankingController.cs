using FarmeORT.DTOs.Ranking;
using FarmeORT.IServicios.Ranking;
using FarmeORT.WebAPI.Filtros;
using Microsoft.AspNetCore.Mvc;

namespace FarmeORT.WebAPI.Controllers;

[ApiController]
[Route("api/v1/rankings")]
public sealed class RankingController(IServicioRanking servicioRanking) : ControllerBase
{
    [HttpGet]
    [AutenticacionOpcional]
    public IActionResult Obtener([FromQuery] FiltroRankingRequest filtro)
    {
        return Ok(servicioRanking.Obtener(filtro));
    }
}
