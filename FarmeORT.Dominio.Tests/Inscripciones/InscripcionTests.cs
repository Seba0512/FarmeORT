using FarmeORT.Dominio.Inscripciones;
using FarmeORT.Dominio.Torneos;
using FarmeORT.Dominio.Usuarios;
using FarmeORT.Excepciones;

namespace FarmeORT.Dominio.Tests.Inscripciones;

[TestClass]
public sealed class InscripcionTests
{
    private static readonly DateOnly FechaActual = new(2026, 10, 1);
    private static readonly DateTime Ahora = new(2026, 10, 1, 12, 30, 0);

    private Torneo _torneo = null!;
    private Usuario _jugador = null!;

    [TestInitialize]
    public void Inicializar()
    {
        var organizador = new Usuario("Laura", "Gomez", "laura@farmeort.com", "laurag", new DateOnly(1990, 5, 20), Rol.Organizador, "hash", FechaActual)
        {
            Id = 2,
        };
        _torneo = new Torneo("COPAAURA", "Copa Aura", "Torneo eliminatorio de batallas de aura.", new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 30), 8, organizador, FechaActual)
        {
            Id = 4,
        };
        _jugador = new Usuario("Ana", "Pérez", "ana@farmeort.com", "anaperez", new DateOnly(2000, 1, 15), Rol.Jugador, "hash", FechaActual)
        {
            Id = 7,
        };
    }

    [TestMethod]
    public void Constructor_TorneoNoAbierto_LanzaTorneoNoAbierto()
    {
        _torneo.Cancelar();

        Assert.ThrowsExactly<TorneoNoAbiertoException>(() => new Inscripcion(_torneo, _jugador, Ahora));
    }
}
