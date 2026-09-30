using System.ComponentModel.DataAnnotations;
using FarmeORT.DTOs.Usuarios;

namespace FarmeORT.WebAPI.Tests.Validaciones;

[TestClass]
public sealed class FiltroUsuariosRequestTests
{
    [TestMethod]
    [DataRow(0)]
    [DataRow(101)]
    [DataRow(-1)]
    public void Validar_LimitFueraDeRango_EsInvalido(int limit)
    {
        var filtro = new FiltroUsuariosRequest { Limit = limit };

        AssertCampoInvalido(filtro, nameof(FiltroUsuariosRequest.Limit));
    }

    [TestMethod]
    public void Validar_OffsetNegativo_EsInvalido()
    {
        var filtro = new FiltroUsuariosRequest { Offset = -1 };

        AssertCampoInvalido(filtro, nameof(FiltroUsuariosRequest.Offset));
    }

    private static void AssertCampoInvalido(FiltroUsuariosRequest filtro, string campo)
    {
        var errores = new List<ValidationResult>();

        var esValido = Validator.TryValidateObject(filtro, new ValidationContext(filtro), errores, validateAllProperties: true);

        Assert.IsFalse(esValido);
        Assert.IsTrue(errores.Any(e => e.MemberNames.Contains(campo)), $"No hay un error para el campo {campo}.");
    }
}
