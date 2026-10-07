using FarmeORT.Dominio.Inscripciones;
using FarmeORT.Dominio.Skills;
using FarmeORT.Dominio.Torneos;
using FarmeORT.Dominio.Usuarios;

namespace FarmeORT.Dominio.Tests.Inscripciones;

// ExigeSkill con el torneo En curso y Finalizado: solo una inscripción de un torneo vigente exige las obligatorias (D-41).
[TestClass]
public sealed class InscripcionFinalizacionTests
{
    private static readonly DateOnly FechaActual = new(2026, 10, 2);

    private Torneo _torneo = null!;
    private Usuario _jugador = null!;
    private Skill _obligatoria = null!;
    private Inscripcion _inscripcion = null!;

    [TestInitialize]
    public void Inicializar()
    {
        var organizador = new Usuario("Laura", "Gomez", "laura@farmeort.com", "laurag", new DateOnly(1990, 5, 20), Rol.Organizador, "hash", FechaActual)
        {
            Id = 2,
        };
        _torneo = new Torneo("COPAAURA", "Copa Aura", "Torneo eliminatorio de batallas de aura.", FechaActual, FechaActual.AddDays(10), 4, organizador, FechaActual);
        _obligatoria = new Skill("SIXSEVEN", "Six-Seven", "Gesto alternando las manos hacia arriba y abajo.", CategoriaSkill.Gestual, 2)
        {
            Id = 3,
        };
        _torneo.AsociarSkillObligatoria(_obligatoria);
        _jugador = new Usuario("Ana", "Pérez", "ana@farmeort.com", "aurafarmer", new DateOnly(2000, 1, 15), Rol.Jugador, "hash", FechaActual)
        {
            Id = 7,
        };
        _jugador.DeclararSkill(_obligatoria, 3, FechaActual);
        _inscripcion = new Inscripcion(_torneo, _jugador, new DateTime(2026, 10, 2));
    }

    [TestMethod]
    public void ExigeSkill_TorneoEnCurso_ExigeLaObligatoria()
    {
        _torneo.Iniciar(4);

        var exige = _inscripcion.ExigeSkill(_obligatoria.Id);

        Assert.IsTrue(exige);
    }

    [TestMethod]
    public void ExigeSkill_TorneoFinalizado_YaNoExigeLaObligatoria()
    {
        _torneo.Iniciar(4);
        _torneo.Finalizar(_jugador);

        var exige = _inscripcion.ExigeSkill(_obligatoria.Id);

        Assert.IsFalse(exige);
    }
}
