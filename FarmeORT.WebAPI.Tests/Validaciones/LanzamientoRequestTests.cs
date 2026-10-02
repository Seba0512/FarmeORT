using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using FarmeORT.DTOs.Batallas;

namespace FarmeORT.WebAPI.Tests.Validaciones;

[TestClass]
public sealed class LanzamientoRequestTests
{
    [TestMethod]
    [DataRow("{}", false)]
    [DataRow("{\"skillId\":null}", false)]
    [DataRow("{\"skillId\":0}", false)]
    [DataRow("{\"skillId\":-1}", false)]
    [DataRow("{\"skillId\":3}", true)]
    public void Validar_JsonDelCliente_ExigeSkillIdPositivo(string json, bool esperado)
    {
        var request = JsonSerializer.Deserialize<LanzamientoRequest>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        Assert.IsNotNull(request);
        var errores = new List<ValidationResult>();

        var valido = Validator.TryValidateObject(request, new ValidationContext(request), errores, validateAllProperties: true);

        Assert.AreEqual(esperado, valido);
        if(!esperado)
        {
            Assert.IsTrue(errores.Any(e => e.MemberNames.Contains(nameof(LanzamientoRequest.SkillId))));
        }
    }
}
