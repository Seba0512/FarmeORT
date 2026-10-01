using FarmeORT.Dominio.Inscripciones;
using FarmeORT.Dominio.Skills;
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

    [TestMethod]
    public void Constructor_JugadorSinTodasLasObligatorias_LanzaSkillsObligatoriasFaltantesConLasQueFaltan()
    {
        var declarada = CrearSkill(3, "SIXSEVEN");
        _torneo.AsociarSkillObligatoria(declarada);
        _torneo.AsociarSkillObligatoria(CrearSkill(5, "TRALALERO"));
        _jugador.DeclararSkill(declarada, 4, FechaActual);

        var excepcion = Assert.ThrowsExactly<SkillsObligatoriasFaltantesException>(() => new Inscripcion(_torneo, _jugador, Ahora));

        CollectionAssert.AreEqual(new[] { "TRALALERO" }, excepcion.Detalles.ToArray());
    }

    [TestMethod]
    public void Constructor_JugadorConTodasLasObligatoriasAunqueUnaSeDesactivo_CreaLaInscripcionAceptada()
    {
        var primera = CrearSkill(3, "SIXSEVEN");
        var segunda = CrearSkill(5, "TRALALERO");
        _torneo.AsociarSkillObligatoria(primera);
        _torneo.AsociarSkillObligatoria(segunda);
        _jugador.DeclararSkill(primera, 4, FechaActual);
        _jugador.DeclararSkill(segunda, 2, FechaActual);
        segunda.Desactivar();

        var inscripcion = new Inscripcion(_torneo, _jugador, Ahora);

        Assert.AreSame(_torneo, inscripcion.Torneo);
        Assert.AreEqual(4, inscripcion.TorneoId);
        Assert.AreSame(_jugador, inscripcion.Jugador);
        Assert.AreEqual(7, inscripcion.JugadorId);
        Assert.AreEqual(Ahora, inscripcion.Fecha);
        Assert.AreEqual(EstadoInscripcion.Aceptada, inscripcion.Estado);
    }

    [TestMethod]
    public void Cancelar_InscripcionAceptada_QuedaCancelada()
    {
        var inscripcion = new Inscripcion(_torneo, _jugador, Ahora);

        inscripcion.Cancelar();

        Assert.AreEqual(EstadoInscripcion.Cancelada, inscripcion.Estado);
    }

    private static Skill CrearSkill(int id, string codigo)
    {
        return new Skill(codigo, "Skill " + id, "Gesto alternando las manos hacia arriba y abajo.", CategoriaSkill.Gestual, 2)
        {
            Id = id,
        };
    }
}
