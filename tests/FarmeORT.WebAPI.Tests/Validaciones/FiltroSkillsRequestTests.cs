using System.ComponentModel.DataAnnotations;
using FarmeORT.DTOs.Skills;

namespace FarmeORT.WebAPI.Tests.Validaciones;

[TestClass]
public sealed class FiltroSkillsRequestTests
{
    [TestMethod]
    [DataRow(null, null, 1, 0)]
    [DataRow(CategoriasSkill.Gestual, 1, 100, 0)]
    [DataRow(CategoriasSkill.Meme, 5, 20, 500)]
    public void Validar_FiltrosYPaginacionEnRango_EsValido(string? categoria, int? dificultad, int limit, int offset)
    {
        var filtro = new FiltroSkillsRequest { Categoria = categoria, Dificultad = dificultad, Limit = limit, Offset = offset };
        var errores = new List<ValidationResult>();

        var esValido = Validator.TryValidateObject(filtro, new ValidationContext(filtro), errores, validateAllProperties: true);

        Assert.IsTrue(esValido, string.Join(" ", errores.Select(e => e.ErrorMessage)));
    }

    [TestMethod]
    public void Constructor_SinParametros_UsaLimit20YOffset0()
    {
        var filtro = new FiltroSkillsRequest();

        Assert.AreEqual(20, filtro.Limit);
        Assert.AreEqual(0, filtro.Offset);
    }

    // Los valores fuera de rango no se rechazan en el borde: el servicio usa el tope, el valor por defecto
    // o ignora el filtro, "para no trancar al usuario" (AGENTS.md §7).
    [TestMethod]
    [DataRow(101, 0, null, null)]
    [DataRow(0, 0, null, null)]
    [DataRow(-1, 0, null, null)]
    [DataRow(20, -1, null, null)]
    [DataRow(20, 0, 0, null)]
    [DataRow(20, 0, 6, null)]
    [DataRow(20, 0, null, "Cocina")]
    public void Validar_ValoresFueraDeRango_NoSeRechazanEnElBorde(int limit, int offset, int? dificultad, string? categoria)
    {
        var filtro = new FiltroSkillsRequest { Limit = limit, Offset = offset, Dificultad = dificultad, Categoria = categoria };
        var errores = new List<ValidationResult>();

        var esValido = Validator.TryValidateObject(filtro, new ValidationContext(filtro), errores, validateAllProperties: true);

        Assert.IsTrue(esValido, string.Join(" ", errores.Select(e => e.ErrorMessage)));
    }
}
