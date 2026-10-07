using FarmeORT.Dominio.Torneos;

namespace FarmeORT.IRepositorios.Torneos;

public sealed record FiltroTorneos(
    EstadoTorneo? Estado,
    DateOnly? FechaDesde,
    DateOnly? FechaHasta,
    string? Nombre,
    int? SkillObligatoriaId,
    bool ExcluirCancelados,
    int Limite,
    int Desplazamiento);
