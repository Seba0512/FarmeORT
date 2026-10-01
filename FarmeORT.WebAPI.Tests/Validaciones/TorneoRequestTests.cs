using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using FarmeORT.DTOs.Torneos;

namespace FarmeORT.WebAPI.Tests.Validaciones;

[TestClass]
public sealed class TorneoRequestTests
{
    [TestMethod]
    [DataRow("ABC12", 5, 20, 4)]
    [DataRow("ABCDEFGHIJ0123456789", 60, 500, 64)]
    [DataRow("CopaAura2026", 20, 100, 16)]
    public void Validar_DatosEnLosLimites_EsValido(string codigo, int largoNombre, int largoDescripcion, int cupo)
    {
        var request = CrearRequest(codigo, new string('a', largoNombre), new string('a', largoDescripcion), cupo);
        var errores = new List<ValidationResult>();

        var esValido = Validator.TryValidateObject(request, new ValidationContext(request), errores, validateAllProperties: true);

        Assert.IsTrue(esValido, string.Join(" ", errores.Select(e => e.ErrorMessage)));
    }

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

    [TestMethod]
    [DataRow(0)]
    [DataRow(4)]
    [DataRow(61)]
    public void Validar_NombreFueraDeRango_EsInvalido(int largo)
    {
        var request = CrearRequest(nombre: new string('a', largo));

        AssertCampoInvalido(request, nameof(TorneoRequest.Nombre));
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(19)]
    [DataRow(501)]
    public void Validar_DescripcionFueraDeRango_EsInvalida(int largo)
    {
        var request = CrearRequest(descripcion: new string('a', largo));

        AssertCampoInvalido(request, nameof(TorneoRequest.Descripcion));
    }

    [TestMethod]
    [DataRow(2)]
    [DataRow(128)]
    public void Validar_CupoFueraDeRango_EsInvalido(int cupo)
    {
        var request = CrearRequest(cupo: cupo);

        AssertCampoInvalido(request, nameof(TorneoRequest.Cupo));
    }

    [TestMethod]
    [DataRow(nameof(TorneoRequest.FechaInicio))]
    [DataRow(nameof(TorneoRequest.FechaFin))]
    [DataRow(nameof(TorneoRequest.Cupo))]
    [DataRow(nameof(TorneoRequest.OrganizadorId))]
    public void Validar_JsonSinDatosObligatorios_RechazaElCampoOmitido(string campo)
    {
        var json = """
            { "codigo": "COPAAURA", "nombre": "Copa Aura",
              "descripcion": "Torneo eliminatorio de batallas de aura." }
            """;
        var request = JsonSerializer.Deserialize<TorneoRequest>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        Assert.IsNotNull(request);
        AssertCampoInvalido(request, campo);
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
