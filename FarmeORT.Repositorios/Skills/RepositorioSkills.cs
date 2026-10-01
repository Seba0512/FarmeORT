using FarmeORT.Dominio.Skills;
using FarmeORT.Excepciones;
using FarmeORT.IRepositorios.Skills;
using Microsoft.EntityFrameworkCore;

namespace FarmeORT.Repositorios.Skills;

public class RepositorioSkills(FarmeORTContext contexto) : IRepositorioSkills
{
    public Skill? ObtenerPorId(int id)
    {
        return contexto.Skills.SingleOrDefault(s => s.Id == id);
    }

    public IReadOnlyList<Skill> Listar(FiltroSkills filtro)
    {
        var consulta = contexto.Skills.AsQueryable();
        if(filtro.Categoria is { } categoria)
        {
            consulta = consulta.Where(s => s.Categoria == categoria);
        }

        if(filtro.Dificultad is { } dificultad)
        {
            consulta = consulta.Where(s => s.Dificultad == dificultad);
        }

        if(filtro.Activa is { } activa)
        {
            consulta = consulta.Where(s => s.Activa == activa);
        }

        if(!string.IsNullOrWhiteSpace(filtro.Nombre))
        {
            var nombre = filtro.Nombre.ToLower();
            consulta = consulta.Where(s => s.Nombre.ToLower().Contains(nombre));
        }

        return consulta
            .OrderBy(s => s.Id)
            .Skip(filtro.Desplazamiento)
            .Take(filtro.Limite)
            .ToList();
    }

    // SQL Server no distingue mayúsculas y SQLite sí: se compara en minúscula para responder igual en los dos.
    public bool ExistePorCodigo(string codigo)
    {
        var codigoBuscado = codigo.ToLower();
        return contexto.Skills.Any(s => s.Codigo.ToLower() == codigoBuscado);
    }

    public void Agregar(Skill skill)
    {
        contexto.Skills.Add(skill);
        Guardar(skill);
    }

    public void Actualizar(Skill skill)
    {
        contexto.Skills.Update(skill);
        Guardar(skill);
    }

    // Otra skill puede ocupar el código entre la validación del servicio y el guardado (D-23).
    private void Guardar(Skill skill)
    {
        try
        {
            contexto.SaveChanges();
        }
        catch(DbUpdateException)
        {
            contexto.Entry(skill).State = EntityState.Detached;
            var codigo = skill.Codigo.ToLower();
            if(contexto.Skills.Any(s => s.Codigo.ToLower() == codigo && s.Id != skill.Id))
            {
                throw new CodigoSkillYaExisteException();
            }

            throw;
        }
    }
}
