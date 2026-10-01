using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using FarmeORT.DTOs.Repertorio;

namespace FarmeORT.WebAPI.Tests.Validaciones;

[TestClass]
public sealed class SkillDeclaradaRequestTests
{
    [TestMethod]
    [DataRow(0)]
    [DataRow(6)]
    public void Validar_NivelFueraDeRango_EsInvalido(int nivel)
    {
        var request = new SkillDeclaradaRequest { SkillId = 3, Nivel = nivel };

        AssertCampoInvalido(request, nameof(SkillDeclaradaRequest.Nivel));
    }

    [TestMethod]
    [DataRow(nameof(SkillDeclaradaRequest.SkillId))]
    [DataRow(nameof(SkillDeclaradaRequest.Nivel))]
    public void Validar_JsonVacio_RechazaElCampoOmitido(string campo)
    {
        var request = JsonSerializer.Deserialize<SkillDeclaradaRequest>("{}", new JsonSerializerOptions(JsonSerializerDefaults.Web));

        Assert.IsNotNull(request);
        AssertCampoInvalido(request, campo);
    }

    private static void AssertCampoInvalido(SkillDeclaradaRequest request, string campo)
    {
        var errores = new List<ValidationResult>();

        var esValido = Validator.TryValidateObject(request, new ValidationContext(request), errores, validateAllProperties: true);

        Assert.IsFalse(esValido);
        Assert.IsTrue(errores.Any(e => e.MemberNames.Contains(campo)), $"No hay un error para el campo {campo}.");
    }
}
