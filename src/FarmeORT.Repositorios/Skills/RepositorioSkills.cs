using FarmeORT.Dominio.Skills;
using FarmeORT.Excepciones;
using FarmeORT.IRepositorios.Skills;
using Microsoft.EntityFrameworkCore;

namespace FarmeORT.Repositorios.Skills;

internal sealed class RepositorioSkills(FarmeORTContext contexto) : IRepositorioSkills
{
    public Skill? ObtenerPorId(int id)
    {
        return contexto.Skills.SingleOrDefault(s => s.Id == id);
    }

    public IReadOnlyList<Skill> Listar(FiltroSkills filtro)
    {
        var consulta = FiltrarPorAtributos(contexto.Skills, filtro);
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
            // Se suelta la skill que falló para que el contexto se pueda seguir usando.
            contexto.Entry(skill).State = EntityState.Detached;
            if(ExisteOtraSkillConElCodigo(skill))
            {
                throw new CodigoSkillYaExisteException();
            }

            throw;
        }
    }

    private bool ExisteOtraSkillConElCodigo(Skill skill)
    {
        var codigo = skill.Codigo.ToLower();
        return contexto.Skills.Any(s => s.Codigo.ToLower() == codigo && s.Id != skill.Id);
    }

    private static IQueryable<Skill> FiltrarPorAtributos(IQueryable<Skill> consulta, FiltroSkills filtro)
    {
        if(filtro.Categoria != null)
        {
            consulta = consulta.Where(s => s.Categoria == filtro.Categoria.Value);
        }

        if(filtro.Dificultad != null)
        {
            consulta = consulta.Where(s => s.Dificultad == filtro.Dificultad.Value);
        }

        if(filtro.Activa != null)
        {
            consulta = consulta.Where(s => s.Activa == filtro.Activa.Value);
        }

        return consulta;
    }
}
