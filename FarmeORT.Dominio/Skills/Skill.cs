namespace FarmeORT.Dominio.Skills;

public sealed class Skill(string codigo, string nombre, string descripcion, CategoriaSkill categoria, int dificultad)
{
    public int Id { get; init; }

    public string Codigo { get; private set; } = codigo;

    public string Nombre { get; private set; } = nombre;

    public string Descripcion { get; private set; } = descripcion;

    public CategoriaSkill Categoria { get; private set; } = categoria;

    public int Dificultad { get; private set; } = dificultad;

    public bool Activa { get; private set; } = true;
}
