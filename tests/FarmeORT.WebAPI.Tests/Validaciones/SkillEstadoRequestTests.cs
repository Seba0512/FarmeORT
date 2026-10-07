using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using FarmeORT.DTOs.Skills;

namespace FarmeORT.WebAPI.Tests.Validaciones;

[TestClass]
public sealed class SkillEstadoRequestTests
{
    [TestMethod]
    public void Validar_JsonSinActiva_RechazaEstadoOmitido()
    {
        var request = JsonSerializer.Deserialize<SkillEstadoRequest>("{}", new JsonSerializerOptions(JsonSerializerDefaults.Web));

        Assert.IsNotNull(request);
        var errores = new List<ValidationResult>();
        var esValido = Validator.TryValidateObject(request, new ValidationContext(request), errores, validateAllProperties: true);

        Assert.IsFalse(esValido);
        Assert.IsTrue(errores.Any(e => e.MemberNames.Contains(nameof(SkillEstadoRequest.Activa))));
    }
}
