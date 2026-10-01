using System.Text.RegularExpressions;
using FarmeORT.Excepciones;

namespace FarmeORT.Dominio.Skills;

public sealed partial class Skill
{
    public Skill(string codigo, string nombre, string descripcion, CategoriaSkill categoria, int dificultad)
    {
        Modificar(codigo, nombre, descripcion, categoria, dificultad);
        Activa = true;
    }

    public int Id { get; init; }

    public string Codigo { get; private set; } = string.Empty;

    public string Nombre { get; private set; } = string.Empty;

    public string Descripcion { get; private set; } = string.Empty;

    public CategoriaSkill Categoria { get; private set; }

    public int Dificultad { get; private set; }

    public bool Activa { get; private set; }

    public void Modificar(string codigo, string nombre, string descripcion, CategoriaSkill categoria, int dificultad)
    {
        ValidarCodigo(codigo);

        Codigo = codigo;
        Nombre = nombre;
        Descripcion = descripcion;
        Categoria = categoria;
        Dificultad = dificultad;
    }

    public void Activar()
    {
        Activa = true;
    }

    public void Desactivar()
    {
        Activa = false;
    }

    private static void ValidarCodigo(string codigo)
    {
        if(!PatronCodigo().IsMatch(codigo))
        {
            throw new DatosInvalidosException("El código debe ser alfanumérico, sin espacios, de 5 a 20 caracteres.");
        }
    }

    [GeneratedRegex("^[a-zA-Z0-9]{5,20}$")]
    private static partial Regex PatronCodigo();
}
