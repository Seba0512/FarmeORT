using System.Text.RegularExpressions;
using FarmeORT.Excepciones;

namespace FarmeORT.Dominio.Skills;

public sealed partial class Skill
{
    private const int LargoMinimoNombre = 3;
    private const int LargoMaximoNombre = 40;
    private const int LargoMinimoDescripcion = 20;
    private const int LargoMaximoDescripcion = 300;
    private const int DificultadMinima = 1;
    private const int DificultadMaxima = 5;

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
        ValidarNombre(nombre);
        ValidarDescripcion(descripcion);
        ValidarCategoria(categoria);
        ValidarDificultad(dificultad);

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

    private static void ValidarNombre(string nombre)
    {
        if(string.IsNullOrWhiteSpace(nombre))
        {
            throw new DatosInvalidosException("El nombre es obligatorio.");
        }

        if(nombre.Length is < LargoMinimoNombre or > LargoMaximoNombre)
        {
            throw new DatosInvalidosException("El nombre debe tener entre 3 y 40 caracteres.");
        }
    }

    private static void ValidarDescripcion(string descripcion)
    {
        if(string.IsNullOrWhiteSpace(descripcion))
        {
            throw new DatosInvalidosException("La descripción es obligatoria.");
        }

        if(descripcion.Length is < LargoMinimoDescripcion or > LargoMaximoDescripcion)
        {
            throw new DatosInvalidosException("La descripción debe tener entre 20 y 300 caracteres.");
        }
    }

    private static void ValidarCategoria(CategoriaSkill categoria)
    {
        if(!Enum.IsDefined(categoria))
        {
            throw new DatosInvalidosException("La categoría debe ser Gestual, Vocal, Baile o Meme.");
        }
    }

    private static void ValidarDificultad(int dificultad)
    {
        if(dificultad is < DificultadMinima or > DificultadMaxima)
        {
            throw new DatosInvalidosException("La dificultad debe ser un entero entre 1 y 5.");
        }
    }

    [GeneratedRegex("^[a-zA-Z0-9]{5,20}$")]
    private static partial Regex PatronCodigo();
}
