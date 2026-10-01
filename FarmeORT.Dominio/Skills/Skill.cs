namespace FarmeORT.Dominio.Skills;

public sealed class Skill
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
        Codigo = codigo;
        Nombre = nombre;
        Descripcion = descripcion;
        Categoria = categoria;
        Dificultad = dificultad;
    }
}
