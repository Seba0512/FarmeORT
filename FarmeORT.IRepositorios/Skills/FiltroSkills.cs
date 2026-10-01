using FarmeORT.Dominio.Skills;

namespace FarmeORT.IRepositorios.Skills;

public record FiltroSkills(
    CategoriaSkill? Categoria,
    int? Dificultad,
    bool? Activa,
    string? Nombre,
    int Limite,
    int Desplazamiento);
