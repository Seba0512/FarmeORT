using FarmeORT.Dominio.Skills;
using FarmeORT.Dominio.Torneos;
using FarmeORT.Dominio.Usuarios;
using FarmeORT.Excepciones;

namespace FarmeORT.Dominio.Tests.Torneos;

[TestClass]
public sealed class SkillsObligatoriasTests
{
    private static readonly DateOnly FechaActual = new(2026, 10, 1);

    private Torneo _torneo = null!;

    [TestInitialize]
    public void Inicializar()
    {
        var organizador = new Usuario("Laura", "Gomez", "laura@farmeort.com", "laurag", new DateOnly(1990, 5, 20), Rol.Organizador, "hash", FechaActual)
        {
            Id = 2,
        };
        _torneo = new Torneo("COPAAURA", "Copa Aura", "Torneo eliminatorio de batallas de aura.", new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 30), 8, organizador, FechaActual);
    }

    [TestMethod]
    public void AsociarSkillObligatoria_TorneoNoAbierto_LanzaTorneoNoModificableSinAsociarla()
    {
        _torneo.Cancelar();

        Assert.ThrowsExactly<TorneoNoModificableException>(() => _torneo.AsociarSkillObligatoria(CrearSkill(3)));
        Assert.IsEmpty(_torneo.SkillsObligatorias);
    }

    [TestMethod]
    public void AsociarSkillObligatoria_SkillInactiva_LanzaSkillInactivaSinAsociarla()
    {
        var skill = CrearSkill(3);
        skill.Desactivar();

        Assert.ThrowsExactly<SkillInactivaException>(() => _torneo.AsociarSkillObligatoria(skill));
        Assert.IsEmpty(_torneo.SkillsObligatorias);
    }

    [TestMethod]
    public void AsociarSkillObligatoria_SkillYaAsociada_LanzaSkillObligatoriaYaAsociadaSinDuplicarla()
    {
        _torneo.AsociarSkillObligatoria(CrearSkill(3));

        Assert.ThrowsExactly<SkillObligatoriaYaAsociadaException>(() => _torneo.AsociarSkillObligatoria(CrearSkill(3)));
        Assert.HasCount(1, _torneo.SkillsObligatorias);
    }

    [TestMethod]
    public void AsociarSkillObligatoria_YaTieneDos_LanzaLimiteSkillsObligatoriasSinAsociarLaTercera()
    {
        _torneo.AsociarSkillObligatoria(CrearSkill(3));
        _torneo.AsociarSkillObligatoria(CrearSkill(5));

        Assert.ThrowsExactly<LimiteSkillsObligatoriasException>(() => _torneo.AsociarSkillObligatoria(CrearSkill(7)));
        CollectionAssert.AreEqual(new[] { 3, 5 }, _torneo.SkillsObligatorias.Select(s => s.Id).ToArray());
    }

    [TestMethod]
    public void QuitarSkillObligatoria_TorneoNoAbierto_LanzaTorneoNoModificableSinQuitarla()
    {
        _torneo.AsociarSkillObligatoria(CrearSkill(3));
        _torneo.Cancelar();

        Assert.ThrowsExactly<TorneoNoModificableException>(() => _torneo.QuitarSkillObligatoria(3));
        Assert.HasCount(1, _torneo.SkillsObligatorias);
    }

    private static Skill CrearSkill(int id)
    {
        return new Skill("SKILL" + id, "Skill " + id, "Gesto alternando las manos hacia arriba y abajo.", CategoriaSkill.Gestual, 2)
        {
            Id = id,
        };
    }
}
