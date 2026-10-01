using FarmeORT.Dominio.Skills;
using FarmeORT.IRepositorios.Skills;
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

    [TestMethod]
    [DataRow("SIXSEVEN")]
    [DataRow("sixseven")]
    public void ExistePorCodigo_CodigoGuardado_DevuelveTrueSinDistinguirMayusculas(string codigo)
    {
        GuardarSkills(CrearSkill("SIXSEVEN"));

        var existe = _repositorio.ExistePorCodigo(codigo);

        Assert.IsTrue(existe);
    }

    [TestMethod]
    public void ExistePorCodigo_CodigoInexistente_DevuelveFalse()
    {
        GuardarSkills(CrearSkill("SIXSEVEN"));

        var existe = _repositorio.ExistePorCodigo("TRALALERO");

        Assert.IsFalse(existe);
    }

    [TestMethod]
    public void Listar_SinFiltros_DevuelveTodasOrdenadasPorId()
    {
        GuardarSkills(CrearSkill("TRALALERO"), CrearSkill("SIXSEVEN"));

        var skills = _repositorio.Listar(SinFiltros());

        CollectionAssert.AreEqual(new[] { "TRALALERO", "SIXSEVEN" }, skills.Select(s => s.Codigo).ToArray());
    }

    [TestMethod]
    public void Listar_FiltroPorCategoria_DevuelveSoloLasDeEsaCategoria()
    {
        GuardarSkills(
            CrearSkill("SIXSEVEN", categoria: CategoriaSkill.Gestual),
            CrearSkill("TRALALERO", categoria: CategoriaSkill.Meme),
            CrearSkill("AURASTARE", categoria: CategoriaSkill.Gestual));

        var skills = _repositorio.Listar(SinFiltros() with { Categoria = CategoriaSkill.Gestual });

        CollectionAssert.AreEqual(new[] { "SIXSEVEN", "AURASTARE" }, skills.Select(s => s.Codigo).ToArray());
    }

    [TestMethod]
    public void Listar_FiltroPorDificultad_DevuelveSoloLasDeEsaDificultad()
    {
        GuardarSkills(
            CrearSkill("SIXSEVEN", dificultad: 2),
            CrearSkill("TRALALERO", dificultad: 5),
            CrearSkill("AURASTARE", dificultad: 2));

        var skills = _repositorio.Listar(SinFiltros() with { Dificultad = 2 });

        CollectionAssert.AreEqual(new[] { "SIXSEVEN", "AURASTARE" }, skills.Select(s => s.Codigo).ToArray());
    }

    private static FiltroSkills SinFiltros()
    {
        return new FiltroSkills(null, null, null, null, 20, 0);
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
