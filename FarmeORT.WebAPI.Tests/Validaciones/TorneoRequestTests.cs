using System.ComponentModel.DataAnnotations;
using FarmeORT.DTOs.Torneos;

namespace FarmeORT.WebAPI.Tests.Validaciones;

[TestClass]
public sealed class TorneoRequestTests
{
    [TestMethod]
    [DataRow("")]
    [DataRow("COPA")]
    [DataRow("ABCDEFGHIJKLMNOPQRSTU")]
    [DataRow("COPA-AURA")]
    [DataRow("COPA AURA")]
    public void Validar_CodigoInvalido_EsInvalido(string codigo)
    {
        var request = CrearRequest(codigo: codigo);

        AssertCampoInvalido(request, nameof(TorneoRequest.Codigo));
    }

    private static TorneoRequest CrearRequest(
        string codigo = "COPAAURA",
        string nombre = "Copa Aura",
        string descripcion = "Torneo eliminatorio de batallas de aura.",
        int? cupo = 8)
    {
        return new TorneoRequest
        {
            Codigo = codigo,
            Nombre = nombre,
            Descripcion = descripcion,
            FechaInicio = new DateOnly(2026, 11, 1),
            FechaFin = new DateOnly(2026, 11, 30),
            Cupo = cupo,
            OrganizadorId = 2,
        };
    }

    private static void AssertCampoInvalido(TorneoRequest request, string campo)
    {
        var errores = new List<ValidationResult>();

        var esValido = Validator.TryValidateObject(request, new ValidationContext(request), errores, validateAllProperties: true);

        Assert.IsFalse(esValido);
        Assert.IsTrue(errores.Any(e => e.MemberNames.Contains(campo)), $"No hay un error para el campo {campo}.");
    }
}
