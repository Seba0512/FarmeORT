using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using FarmeORT.DTOs.Inscripciones;

namespace FarmeORT.WebAPI.Tests.Validaciones;

[TestClass]
public sealed class InscripcionRequestTests
{
    [TestMethod]
    public void Validar_JsonVacio_RechazaElTorneoOmitido()
    {
        var request = JsonSerializer.Deserialize<InscripcionRequest>("{}", new JsonSerializerOptions(JsonSerializerDefaults.Web));
        var errores = new List<ValidationResult>();

        Assert.IsNotNull(request);
        var esValido = Validator.TryValidateObject(request, new ValidationContext(request), errores, validateAllProperties: true);

        Assert.IsFalse(esValido);
        Assert.IsTrue(errores.Any(e => e.MemberNames.Contains(nameof(InscripcionRequest.TorneoId))));
    }
}
