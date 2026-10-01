using FarmeORT.Dominio.Skills;
using FarmeORT.Repositorios.Skills;

namespace FarmeORT.Repositorios.Tests.Skills;

[TestClass]
public sealed class RepositorioSkillsTests
{
    private const string Descripcion = "Gesto alternando las manos hacia arriba y abajo.";

    private BaseDeDatosEnMemoria _baseDeDatos = null!;
    private RepositorioSkills _repositorio = null!;

    [TestInitialize]
    public void Inicializar()
    {
        _baseDeDatos = new BaseDeDatosEnMemoria();
        _repositorio = new RepositorioSkills(_baseDeDatos.Contexto);
    }

    [TestCleanup]
    public void Limpiar()
    {
        _baseDeDatos.Dispose();
    }

    [TestMethod]
    public void Agregar_SkillNueva_QuedaGuardadaConSuId()
    {
        var skill = new Skill("SIXSEVEN", "Six-Seven", Descripcion, CategoriaSkill.Gestual, 2);

        _repositorio.Agregar(skill);
        _baseDeDatos.Contexto.ChangeTracker.Clear();

        Assert.IsGreaterThan(0, skill.Id);
        var guardada = _baseDeDatos.Contexto.Skills.SingleOrDefault(s => s.Id == skill.Id);
        Assert.IsNotNull(guardada);
        Assert.AreEqual("SIXSEVEN", guardada.Codigo);
        Assert.AreEqual("Six-Seven", guardada.Nombre);
        Assert.AreEqual(Descripcion, guardada.Descripcion);
        Assert.AreEqual(CategoriaSkill.Gestual, guardada.Categoria);
        Assert.AreEqual(2, guardada.Dificultad);
        Assert.IsTrue(guardada.Activa);
    }
}
