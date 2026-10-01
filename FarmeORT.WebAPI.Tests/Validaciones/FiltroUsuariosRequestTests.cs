using System.ComponentModel.DataAnnotations;
using FarmeORT.DTOs.Usuarios;

namespace FarmeORT.WebAPI.Tests.Validaciones;

[TestClass]
public sealed class FiltroUsuariosRequestTests
{
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(100, 0)]
    [DataRow(20, 500)]
    public void Validar_PaginacionEnRango_EsValida(int limit, int offset)
    {
        var filtro = new FiltroUsuariosRequest { Limit = limit, Offset = offset };
        var errores = new List<ValidationResult>();

        var esValido = Validator.TryValidateObject(filtro, new ValidationContext(filtro), errores, validateAllProperties: true);

        Assert.IsTrue(esValido);
    }

    [TestMethod]
    public void Constructor_SinParametros_UsaLimit20YOffset0()
    {
        var filtro = new FiltroUsuariosRequest();

        Assert.AreEqual(20, filtro.Limit);
        Assert.AreEqual(0, filtro.Offset);
    }

    // La paginación fuera de rango no se rechaza en el borde: el servicio usa el tope, el valor por defecto
    // o empieza desde el principio, "para no trancar al usuario" (AGENTS.md §7).
    [TestMethod]
    [DataRow(101, 0)]
    [DataRow(0, 0)]
    [DataRow(-1, 0)]
    [DataRow(20, -1)]
    public void Validar_PaginacionFueraDeRango_NoSeRechazaEnElBorde(int limit, int offset)
    {
        var filtro = new FiltroUsuariosRequest { Limit = limit, Offset = offset };
        var errores = new List<ValidationResult>();

        var esValido = Validator.TryValidateObject(filtro, new ValidationContext(filtro), errores, validateAllProperties: true);

        Assert.IsTrue(esValido, string.Join(" ", errores.Select(e => e.ErrorMessage)));
    }
}
