using FarmeORT.Dominio.Skills;
using FarmeORT.Dominio.Usuarios;
using FarmeORT.Excepciones;

namespace FarmeORT.Dominio.Tests.Usuarios;

[TestClass]
public sealed class RepertorioTests
{
    private const string Descripcion = "Gesto alternando las manos hacia arriba y abajo.";

    private static readonly DateOnly FechaActual = new(2026, 10, 1);

    private Usuario _jugador = null!;

    [TestInitialize]
    public void Inicializar()
    {
        _jugador = new Usuario("Ana", "Pérez", "ana@farmeort.com", "anaperez", new DateOnly(2000, 1, 15), Rol.Jugador, "hash", FechaActual)
        {
            Id = 7,
        };
    }

    [TestMethod]
    public void DeclararSkill_SkillInactiva_LanzaSkillInactivaSinAgregarla()
    {
        var skill = CrearSkill(3);
        skill.Desactivar();

        Assert.ThrowsExactly<SkillInactivaException>(() => _jugador.DeclararSkill(skill, 4, FechaActual));
        Assert.IsEmpty(_jugador.Repertorio);
    }

    private static Skill CrearSkill(int id)
    {
        return new Skill("SIXSEVEN", "Six-Seven", Descripcion, CategoriaSkill.Gestual, 2)
        {
            Id = id,
        };
    }
}
