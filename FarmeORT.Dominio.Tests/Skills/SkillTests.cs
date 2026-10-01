using FarmeORT.Dominio.Skills;

namespace FarmeORT.Dominio.Tests.Skills;

[TestClass]
public sealed class SkillTests
{
    private const string Descripcion = "Gesto alternando las manos hacia arriba y abajo.";

    [TestMethod]
    public void Constructor_DatosValidos_ConservaLosDatosYQuedaActiva()
    {
        var skill = new Skill("SIXSEVEN", "Six-Seven", Descripcion, CategoriaSkill.Gestual, 2);

        Assert.AreEqual("SIXSEVEN", skill.Codigo);
        Assert.AreEqual("Six-Seven", skill.Nombre);
        Assert.AreEqual(Descripcion, skill.Descripcion);
        Assert.AreEqual(CategoriaSkill.Gestual, skill.Categoria);
        Assert.AreEqual(2, skill.Dificultad);
        Assert.IsTrue(skill.Activa);
    }

    [TestMethod]
    public void Modificar_DatosValidos_ReemplazaLosDatosSinCambiarElEstado()
    {
        var skill = CrearSkill();
        skill.Desactivar();

        skill.Modificar("TRALALERO", "Tralalero", "Canto con ritmo de tarantela italiana.", CategoriaSkill.Meme, 5);

        Assert.AreEqual("TRALALERO", skill.Codigo);
        Assert.AreEqual("Tralalero", skill.Nombre);
        Assert.AreEqual("Canto con ritmo de tarantela italiana.", skill.Descripcion);
        Assert.AreEqual(CategoriaSkill.Meme, skill.Categoria);
        Assert.AreEqual(5, skill.Dificultad);
        Assert.IsFalse(skill.Activa);
    }

    [TestMethod]
    public void Desactivar_SkillActiva_QuedaInactiva()
    {
        var skill = CrearSkill();

        skill.Desactivar();

        Assert.IsFalse(skill.Activa);
    }

    [TestMethod]
    public void Activar_SkillInactiva_QuedaActiva()
    {
        var skill = CrearSkill();
        skill.Desactivar();

        skill.Activar();

        Assert.IsTrue(skill.Activa);
    }

    private static Skill CrearSkill(
        string codigo = "SIXSEVEN",
        string nombre = "Six-Seven",
        string descripcion = Descripcion,
        CategoriaSkill categoria = CategoriaSkill.Gestual,
        int dificultad = 2)
    {
        return new Skill(codigo, nombre, descripcion, categoria, dificultad);
    }
}
