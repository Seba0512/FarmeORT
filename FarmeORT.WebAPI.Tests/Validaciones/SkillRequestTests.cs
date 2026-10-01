using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using FarmeORT.DTOs.Skills;

namespace FarmeORT.WebAPI.Tests.Validaciones;

[TestClass]
public sealed class SkillRequestTests
{
    [TestMethod]
    [DataRow("ABC12", 3, 20, 1, CategoriasSkill.Gestual)]
    [DataRow("ABCDEFGHIJ0123456789", 40, 300, 5, CategoriasSkill.Vocal)]
    [DataRow("SixSeven67", 10, 50, 3, CategoriasSkill.Baile)]
    [DataRow("Tralalero", 10, 50, 2, CategoriasSkill.Meme)]
    public void Validar_DatosEnLosLimites_EsValido(string codigo, int largoNombre, int largoDescripcion, int dificultad, string categoria)
    {
        var request = CrearRequest(codigo, new string('a', largoNombre), new string('a', largoDescripcion), categoria, dificultad);
        var errores = new List<ValidationResult>();

        var esValido = Validator.TryValidateObject(request, new ValidationContext(request), errores, validateAllProperties: true);

        Assert.IsTrue(esValido, string.Join(" ", errores.Select(e => e.ErrorMessage)));
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("SIX7")]
    [DataRow("ABCDEFGHIJKLMNOPQRSTU")]
    [DataRow("SIX-SEVEN")]
    [DataRow("SIX SEVEN")]
    public void Validar_CodigoInvalido_EsInvalido(string codigo)
    {
        var request = CrearRequest(codigo: codigo);

        AssertCampoInvalido(request, nameof(SkillRequest.Codigo));
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(2)]
    [DataRow(41)]
    public void Validar_NombreFueraDeRango_EsInvalido(int largo)
    {
        var request = CrearRequest(nombre: new string('a', largo));

        AssertCampoInvalido(request, nameof(SkillRequest.Nombre));
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(19)]
    [DataRow(301)]
    public void Validar_DescripcionFueraDeRango_EsInvalida(int largo)
    {
        var request = CrearRequest(descripcion: new string('a', largo));

        AssertCampoInvalido(request, nameof(SkillRequest.Descripcion));
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("Cocina")]
    [DataRow("gestual")]
    public void Validar_CategoriaInexistente_EsInvalida(string categoria)
    {
        var request = CrearRequest(categoria: categoria);

        AssertCampoInvalido(request, nameof(SkillRequest.Categoria));
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(6)]
    public void Validar_DificultadFueraDeRango_EsInvalida(int dificultad)
    {
        var request = CrearRequest(dificultad: dificultad);

        AssertCampoInvalido(request, nameof(SkillRequest.Dificultad));
    }

    [TestMethod]
    public void Validar_JsonSinDificultad_RechazaDificultadOmitida()
    {
        var json = """
            { "codigo": "SIXSEVEN", "nombre": "Six-Seven", "categoria": "Gestual",
              "descripcion": "Gesto alternando las manos hacia arriba y abajo." }
            """;
        var request = JsonSerializer.Deserialize<SkillRequest>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        Assert.IsNotNull(request);
        AssertCampoInvalido(request, nameof(SkillRequest.Dificultad));
    }

    private static SkillRequest CrearRequest(
        string codigo = "SIXSEVEN",
        string nombre = "Six-Seven",
        string descripcion = "Gesto alternando las manos hacia arriba y abajo.",
        string categoria = CategoriasSkill.Gestual,
        int? dificultad = 2)
    {
        return new SkillRequest
        {
            Codigo = codigo,
            Nombre = nombre,
            Descripcion = descripcion,
            Categoria = categoria,
            Dificultad = dificultad,
        };
    }

    private static void AssertCampoInvalido(SkillRequest request, string campo)
    {
        var errores = new List<ValidationResult>();

        var esValido = Validator.TryValidateObject(request, new ValidationContext(request), errores, validateAllProperties: true);

        Assert.IsFalse(esValido);
        Assert.IsTrue(errores.Any(e => e.MemberNames.Contains(campo)), $"No hay un error para el campo {campo}.");
    }
}
