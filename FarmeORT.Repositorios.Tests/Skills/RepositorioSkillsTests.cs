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

    [TestMethod]
    public void ObtenerPorId_IdExistente_DevuelveLaSkill()
    {
        var otra = CrearSkill("TRALALERO");
        var skill = CrearSkill("SIXSEVEN");
        GuardarSkills(otra, skill);

        var resultado = _repositorio.ObtenerPorId(skill.Id);

        Assert.IsNotNull(resultado);
        Assert.AreEqual(skill.Id, resultado.Id);
        Assert.AreEqual("SIXSEVEN", resultado.Codigo);
    }

    [TestMethod]
    public void ObtenerPorId_IdInexistente_DevuelveNull()
    {
        var resultado = _repositorio.ObtenerPorId(99);

        Assert.IsNull(resultado);
    }

    private void GuardarSkills(params Skill[] skills)
    {
        _baseDeDatos.Contexto.Skills.AddRange(skills);
        _baseDeDatos.Contexto.SaveChanges();
        _baseDeDatos.Contexto.ChangeTracker.Clear();
    }

    private static Skill CrearSkill(
        string codigo,
        string nombre = "Six-Seven",
        CategoriaSkill categoria = CategoriaSkill.Gestual,
        int dificultad = 2)
    {
        return new Skill(codigo, nombre, Descripcion, categoria, dificultad);
    }
}
