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

        Assert.ThrowsExactly<TorneoNoAbiertoException>(() => torneo.Iniciar(4));
    }

    [TestMethod]
    public void Iniciar_TorneoYaEnCurso_LanzaTorneoNoAbierto()
    {
        var torneo = CrearTorneo();
        torneo.Iniciar(4);

        Assert.ThrowsExactly<TorneoNoAbiertoException>(() => torneo.Iniciar(4));
        Assert.AreEqual(EstadoTorneo.EnCurso, torneo.Estado);
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(3)]
    public void Iniciar_MenosAceptadasQueElCupo_LanzaTorneoIncompleto(int inscripcionesAceptadas)
    {
        var torneo = CrearTorneo();

        Assert.ThrowsExactly<TorneoIncompletoException>(() => torneo.Iniciar(inscripcionesAceptadas));
        Assert.AreEqual(EstadoTorneo.Abierto, torneo.Estado);
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
