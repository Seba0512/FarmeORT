using FarmeORT.Dominio.Skills;
using FarmeORT.Excepciones;
using FarmeORT.IRepositorios.Skills;
using FarmeORT.Repositorios.Skills;
using Microsoft.EntityFrameworkCore;

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

        var skills = _repositorio.Listar(Filtro());

        CollectionAssert.AreEqual(new[] { "TRALALERO", "SIXSEVEN" }, skills.Select(s => s.Codigo).ToArray());
    }

    [TestMethod]
    public void Listar_FiltroPorCategoria_DevuelveSoloLasDeEsaCategoria()
    {
        GuardarSkills(
            CrearSkill("SIXSEVEN", categoria: CategoriaSkill.Gestual),
            CrearSkill("TRALALERO", categoria: CategoriaSkill.Meme),
            CrearSkill("AURASTARE", categoria: CategoriaSkill.Gestual));

        var skills = _repositorio.Listar(Filtro(categoria: CategoriaSkill.Gestual));

        CollectionAssert.AreEqual(new[] { "SIXSEVEN", "AURASTARE" }, skills.Select(s => s.Codigo).ToArray());
    }

    [TestMethod]
    public void Listar_FiltroPorDificultad_DevuelveSoloLasDeEsaDificultad()
    {
        GuardarSkills(
            CrearSkill("SIXSEVEN", dificultad: 2),
            CrearSkill("TRALALERO", dificultad: 5),
            CrearSkill("AURASTARE", dificultad: 2));

        var skills = _repositorio.Listar(Filtro(dificultad: 2));

        CollectionAssert.AreEqual(new[] { "SIXSEVEN", "AURASTARE" }, skills.Select(s => s.Codigo).ToArray());
    }

    [TestMethod]
    [DataRow(true, "SIXSEVEN")]
    [DataRow(false, "TRALALERO")]
    public void Listar_FiltroPorEstado_DevuelveSoloLasDeEseEstado(bool activa, string codigoEsperado)
    {
        var inactiva = CrearSkill("TRALALERO");
        inactiva.Desactivar();
        GuardarSkills(CrearSkill("SIXSEVEN"), inactiva);

        var skills = _repositorio.Listar(Filtro(activa: activa));

        CollectionAssert.AreEqual(new[] { codigoEsperado }, skills.Select(s => s.Codigo).ToArray());
    }

    [TestMethod]
    public void Listar_FiltroPorNombre_DevuelveLasQueLoContienenSinDistinguirMayusculas()
    {
        GuardarSkills(
            CrearSkill("SIXSEVEN", nombre: "Six-Seven"),
            CrearSkill("TRALALERO", nombre: "Tralalero"),
            CrearSkill("SIXSEVENPRO", nombre: "Six-Seven Pro"));

        var skills = _repositorio.Listar(Filtro(nombre: "SEVEN"));

        CollectionAssert.AreEqual(new[] { "SIXSEVEN", "SIXSEVENPRO" }, skills.Select(s => s.Codigo).ToArray());
    }

    [TestMethod]
    public void Listar_ConLimiteYDesplazamiento_DevuelveLaPaginaEnOrdenDeId()
    {
        GuardarSkills(CrearSkill("SKILLUNO"), CrearSkill("SKILLDOS"), CrearSkill("SKILLTRES"), CrearSkill("SKILLCUATRO"));

        var skills = _repositorio.Listar(Filtro(limite: 2, desplazamiento: 1));

        CollectionAssert.AreEqual(new[] { "SKILLDOS", "SKILLTRES" }, skills.Select(s => s.Codigo).ToArray());
    }

    [TestMethod]
    public void Listar_VariosFiltros_DevuelveLasQueCumplenTodos()
    {
        GuardarSkills(
            CrearSkill("SIXSEVEN", nombre: "Six-Seven", categoria: CategoriaSkill.Gestual, dificultad: 2),
            CrearSkill("SIXSEVENPRO", nombre: "Six-Seven Pro", categoria: CategoriaSkill.Gestual, dificultad: 4),
            CrearSkill("SIXSEVENVOZ", nombre: "Six-Seven cantado", categoria: CategoriaSkill.Vocal, dificultad: 2));

        var filtro = Filtro(categoria: CategoriaSkill.Gestual, dificultad: 2, activa: true, nombre: "six");
        var skills = _repositorio.Listar(filtro);

        CollectionAssert.AreEqual(new[] { "SIXSEVEN" }, skills.Select(s => s.Codigo).ToArray());
    }

    [TestMethod]
    public void Actualizar_SkillModificada_GuardaLosCambios()
    {
        var skill = GuardarYCargar(CrearSkill("SIXSEVEN"));
        skill.Modificar("TRALALERO", "Tralalero", "Canto con ritmo de tarantela italiana.", CategoriaSkill.Meme, 5);
        skill.Desactivar();

        _repositorio.Actualizar(skill);
        _baseDeDatos.Contexto.ChangeTracker.Clear();

        var guardada = _baseDeDatos.Contexto.Skills.Single(s => s.Id == skill.Id);
        Assert.AreEqual("TRALALERO", guardada.Codigo);
        Assert.AreEqual("Tralalero", guardada.Nombre);
        Assert.AreEqual("Canto con ritmo de tarantela italiana.", guardada.Descripcion);
        Assert.AreEqual(CategoriaSkill.Meme, guardada.Categoria);
        Assert.AreEqual(5, guardada.Dificultad);
        Assert.IsFalse(guardada.Activa);
    }

    [TestMethod]
    public void Agregar_CodigoOcupadoDespuesDeConsultar_LanzaCodigoSkillYaExisteSinDuplicar()
    {
        Assert.IsFalse(_repositorio.ExistePorCodigo("SIXSEVEN"));
        GuardarSkills(CrearSkill("SIXSEVEN"));
        var nueva = CrearSkill("SIXSEVEN", nombre: "Otra skill");

        Assert.ThrowsExactly<CodigoSkillYaExisteException>(() => _repositorio.Agregar(nueva));
        Assert.AreEqual(1, _baseDeDatos.Contexto.Skills.Count(s => s.Codigo == "SIXSEVEN"));
        AssertPuedeGuardarOtraSkill();
    }

    [TestMethod]
    public void Agregar_ErrorDeBaseSinCodigoDuplicado_PropagaDbUpdateException()
    {
        var existente = CrearSkill("SIXSEVEN");
        GuardarSkills(existente);
        var nueva = new Skill("TRALALERO", "Tralalero", Descripcion, CategoriaSkill.Meme, 3) { Id = existente.Id };

        Assert.ThrowsExactly<DbUpdateException>(() => _repositorio.Agregar(nueva));
        Assert.IsFalse(_baseDeDatos.Contexto.Skills.Any(s => s.Codigo == "TRALALERO"));
        AssertPuedeGuardarOtraSkill();
    }

    [TestMethod]
    public void Actualizar_CodigoOcupadoDespuesDeConsultar_LanzaCodigoSkillYaExisteSinCambiarLaSkill()
    {
        GuardarSkills(CrearSkill("TRALALERO"));
        var skill = GuardarYCargar(CrearSkill("SIXSEVEN"));
        skill.Modificar("TRALALERO", "Six-Seven", Descripcion, CategoriaSkill.Gestual, 2);

        Assert.ThrowsExactly<CodigoSkillYaExisteException>(() => _repositorio.Actualizar(skill));
        Assert.AreEqual("SIXSEVEN", _baseDeDatos.Contexto.Skills.AsNoTracking().Single(s => s.Id == skill.Id).Codigo);
    }

    // Arma el filtro del repositorio indicando solo lo que cada test necesita; el resto no filtra.
    private static FiltroSkills Filtro(
        CategoriaSkill? categoria = null,
        int? dificultad = null,
        bool? activa = null,
        string? nombre = null,
        int limite = 20,
        int desplazamiento = 0)
    {
        return new FiltroSkills(categoria, dificultad, activa, nombre, limite, desplazamiento);
    }

    private void AssertPuedeGuardarOtraSkill()
    {
        var siguiente = CrearSkill("SIGUIENTE");

        _repositorio.Agregar(siguiente);

        Assert.IsTrue(_baseDeDatos.Contexto.Skills.Any(s => s.Id == siguiente.Id && s.Codigo == "SIGUIENTE"));
    }

    // Deja la skill guardada y cargada por el repositorio, como la recibe el servicio antes de modificarla.
    private Skill GuardarYCargar(Skill skill)
    {
        GuardarSkills(skill);
        return _repositorio.ObtenerPorId(skill.Id)!;
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
