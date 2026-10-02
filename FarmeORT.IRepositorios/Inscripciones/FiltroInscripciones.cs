using FarmeORT.Dominio.Inscripciones;

namespace FarmeORT.IRepositorios.Inscripciones;

// JugadorId y OrganizadorId restringen lo que ve el usuario según su rol (letra 1.6); no vienen del cliente.
public sealed record FiltroInscripciones(
    int? TorneoId,
    EstadoInscripcion? Estado,
    DateOnly? FechaDesde,
    DateOnly? FechaHasta,
    int? JugadorId,
    int? OrganizadorId,
    int Limite,
    int Desplazamiento);
