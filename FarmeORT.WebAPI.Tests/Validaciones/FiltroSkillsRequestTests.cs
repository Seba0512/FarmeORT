using System.ComponentModel.DataAnnotations;
using FarmeORT.DTOs.Skills;

namespace FarmeORT.WebAPI.Tests.Validaciones;

[TestClass]
public sealed class FiltroSkillsRequestTests
{
    [TestMethod]
    public void Constructor_SinParametros_UsaLimit20YOffset0()
    {
        var filtro = new FiltroSkillsRequest();

        Assert.AreEqual(20, filtro.Limit);
        Assert.AreEqual(0, filtro.Offset);
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(101)]
    [DataRow(-1)]
    public void Validar_LimitFueraDeRango_EsInvalido(int limit)
    {
        var filtro = new FiltroSkillsRequest { Limit = limit };

        AssertCampoInvalido(filtro, nameof(FiltroSkillsRequest.Limit));
    }

    [TestMethod]
    public void Validar_OffsetNegativo_EsInvalido()
    {
        var filtro = new FiltroSkillsRequest { Limit = 20, Offset = -1 };

        AssertCampoInvalido(filtro, nameof(FiltroSkillsRequest.Offset));
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(6)]
    public void Validar_DificultadFueraDeRango_EsInvalida(int dificultad)
    {
        var filtro = new FiltroSkillsRequest { Dificultad = dificultad, Limit = 20 };

        AssertCampoInvalido(filtro, nameof(FiltroSkillsRequest.Dificultad));
    }

    [TestMethod]
    [DataRow("Cocina")]
    [DataRow("gestual")]
    public void Validar_CategoriaInexistente_EsInvalida(string categoria)
    {
        var filtro = new FiltroSkillsRequest { Categoria = categoria, Limit = 20 };

        AssertCampoInvalido(filtro, nameof(FiltroSkillsRequest.Categoria));
    }

    private static void AssertCampoInvalido(FiltroSkillsRequest filtro, string campo)
    {
        var errores = new List<ValidationResult>();

        var esValido = Validator.TryValidateObject(filtro, new ValidationContext(filtro), errores, validateAllProperties: true);

        Assert.IsFalse(esValido);
        Assert.IsTrue(errores.Any(e => e.MemberNames.Contains(campo)), $"No hay un error para el campo {campo}.");
    }
}
