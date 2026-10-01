using FarmeORT.Dominio.Skills;
using FarmeORT.Excepciones;

namespace FarmeORT.Dominio.Tests.Skills;

[TestClass]
public class SkillTests
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

    [TestMethod]
    [DataRow("")]
    [DataRow("SIX7")]
    [DataRow("ABCDEFGHIJKLMNOPQRSTU")]
    [DataRow("SIX-SEVEN")]
    [DataRow("SIX SEVEN")]
    public void Constructor_CodigoInvalido_LanzaDatosInvalidos(string codigo)
    {
        var excepcion = Assert.ThrowsExactly<DatosInvalidosException>(() => CrearSkill(codigo: codigo));

        Assert.AreEqual("El código debe ser alfanumérico, sin espacios, de 5 a 20 caracteres.", excepcion.Message);
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("     ")]
    public void Constructor_NombreVacio_LanzaDatosInvalidos(string nombre)
    {
        var excepcion = Assert.ThrowsExactly<DatosInvalidosException>(() => CrearSkill(nombre: nombre));

        Assert.AreEqual("El nombre es obligatorio.", excepcion.Message);
    }

    [TestMethod]
    [DataRow(2)]
    [DataRow(41)]
    public void Constructor_NombreFueraDeRango_LanzaDatosInvalidos(int largo)
    {
        var excepcion = Assert.ThrowsExactly<DatosInvalidosException>(() => CrearSkill(nombre: new string('a', largo)));

        Assert.AreEqual("El nombre debe tener entre 3 y 40 caracteres.", excepcion.Message);
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("                         ")]
    public void Constructor_DescripcionVacia_LanzaDatosInvalidos(string descripcion)
    {
        var excepcion = Assert.ThrowsExactly<DatosInvalidosException>(() => CrearSkill(descripcion: descripcion));

        Assert.AreEqual("La descripción es obligatoria.", excepcion.Message);
    }

    [TestMethod]
    [DataRow(19)]
    [DataRow(301)]
    public void Constructor_DescripcionFueraDeRango_LanzaDatosInvalidos(int largo)
    {
        var excepcion = Assert.ThrowsExactly<DatosInvalidosException>(() => CrearSkill(descripcion: new string('a', largo)));

        Assert.AreEqual("La descripción debe tener entre 20 y 300 caracteres.", excepcion.Message);
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(6)]
    public void Constructor_DificultadFueraDeRango_LanzaDatosInvalidos(int dificultad)
    {
        var excepcion = Assert.ThrowsExactly<DatosInvalidosException>(() => CrearSkill(dificultad: dificultad));

        Assert.AreEqual("La dificultad debe ser un entero entre 1 y 5.", excepcion.Message);
    }

    [TestMethod]
    public void Constructor_CategoriaInexistente_LanzaDatosInvalidos()
    {
        var excepcion = Assert.ThrowsExactly<DatosInvalidosException>(() => CrearSkill(categoria: (CategoriaSkill)99));

        Assert.AreEqual("La categoría debe ser Gestual, Vocal, Baile o Meme.", excepcion.Message);
    }

    [TestMethod]
    [DataRow("ABC12", 3, 20, 1)]
    [DataRow("ABCDEFGHIJ0123456789", 40, 300, 5)]
    public void Constructor_DatosEnLosLimites_CreaLaSkill(string codigo, int largoNombre, int largoDescripcion, int dificultad)
    {
        var skill = CrearSkill(codigo, new string('a', largoNombre), new string('a', largoDescripcion), CategoriaSkill.Baile, dificultad);

        Assert.AreEqual(codigo, skill.Codigo);
        Assert.AreEqual(largoNombre, skill.Nombre.Length);
        Assert.AreEqual(largoDescripcion, skill.Descripcion.Length);
        Assert.AreEqual(dificultad, skill.Dificultad);
    }

    [TestMethod]
    public void Modificar_DatosInvalidos_LanzaDatosInvalidosSinCambiarLaSkill()
    {
        var skill = CrearSkill();

        Assert.ThrowsExactly<DatosInvalidosException>(() => skill.Modificar("TRALALERO", "Tralalero", "Corta", CategoriaSkill.Meme, 5));

        Assert.AreEqual("SIXSEVEN", skill.Codigo);
        Assert.AreEqual("Six-Seven", skill.Nombre);
        Assert.AreEqual(Descripcion, skill.Descripcion);
        Assert.AreEqual(CategoriaSkill.Gestual, skill.Categoria);
        Assert.AreEqual(2, skill.Dificultad);
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
