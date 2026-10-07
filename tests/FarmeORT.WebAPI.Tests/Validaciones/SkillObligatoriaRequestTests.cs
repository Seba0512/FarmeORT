using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using FarmeORT.DTOs.Torneos;

namespace FarmeORT.WebAPI.Tests.Validaciones;

[TestClass]
public sealed class SkillObligatoriaRequestTests
{
    [TestMethod]
    public void Validar_JsonVacio_RechazaLaSkillOmitida()
    {
        var request = JsonSerializer.Deserialize<SkillObligatoriaRequest>("{}", new JsonSerializerOptions(JsonSerializerDefaults.Web));
        var errores = new List<ValidationResult>();

        Assert.IsNotNull(request);
        var esValido = Validator.TryValidateObject(request, new ValidationContext(request), errores, validateAllProperties: true);

        Assert.IsFalse(esValido);
        Assert.IsTrue(errores.Any(e => e.MemberNames.Contains(nameof(SkillObligatoriaRequest.SkillId))));
    }
}
