using FarmeORT.Dominio.Torneos;
using FarmeORT.Dominio.Usuarios;
using FarmeORT.Excepciones;

namespace FarmeORT.Dominio.Tests.Torneos;

[TestClass]
public sealed class TorneoRondasTests
{
    private static readonly DateOnly FechaActual = new(2026, 10, 2);

    [TestMethod]
    public void Iniciar_TorneoCancelado_LanzaTorneoNoAbierto()
    {
        var torneo = CrearTorneo();
        torneo.Cancelar();

        Assert.ThrowsExactly<TorneoNoAbiertoException>(torneo.Iniciar);
    }

    private static Torneo CrearTorneo()
    {
        var organizador = new Usuario("Laura", "Gomez", "laura@farmeort.com", "laurag", new DateOnly(1990, 5, 20), Rol.Organizador, "hash", FechaActual)
        {
            Id = 2,
        };
        return new Torneo("COPAAURA", "Copa Aura", "Torneo eliminatorio de batallas de aura.", FechaActual, FechaActual.AddDays(10), 4, organizador, FechaActual);
    }
}
