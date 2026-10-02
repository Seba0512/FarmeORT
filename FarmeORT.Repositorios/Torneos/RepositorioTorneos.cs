using FarmeORT.Dominio.Torneos;
using FarmeORT.Excepciones;
using FarmeORT.IRepositorios.Torneos;
using Microsoft.EntityFrameworkCore;

namespace FarmeORT.Repositorios.Torneos;

internal sealed class RepositorioTorneos(FarmeORTContext contexto) : IRepositorioTorneos
{
    // Se incluyen el organizador, porque la respuesta muestra su nombre, y las skills obligatorias.
    public Torneo? ObtenerPorId(int id)
    {
        return contexto.Torneos
            .Include(t => t.Organizador)
            .Include(t => t.SkillsObligatorias)
            .SingleOrDefault(t => t.Id == id);
    }

    public IReadOnlyList<Torneo> Listar(FiltroTorneos filtro)
    {
        var consulta = contexto.Torneos.Include(t => t.Organizador).AsQueryable();
        if(filtro.Estado != null)
        {
            consulta = consulta.Where(t => t.Estado == filtro.Estado.Value);
        }

        if(filtro.ExcluirCancelados)
        {
            consulta = consulta.Where(t => t.Estado != EstadoTorneo.Cancelado);
        }

        if(filtro.FechaDesde != null)
        {
            consulta = consulta.Where(t => t.FechaInicio >= filtro.FechaDesde.Value);
        }

        if(filtro.FechaHasta != null)
        {
            consulta = consulta.Where(t => t.FechaInicio <= filtro.FechaHasta.Value);
        }

        if(!string.IsNullOrWhiteSpace(filtro.Nombre))
        {
            var nombre = filtro.Nombre.ToLower();
            consulta = consulta.Where(t => t.Nombre.ToLower().Contains(nombre));
        }

        if(filtro.SkillObligatoriaId != null)
        {
            consulta = consulta.Where(t => t.SkillsObligatorias.Any(s => s.Id == filtro.SkillObligatoriaId.Value));
        }

        return consulta
            .OrderBy(t => t.Id)
            .Skip(filtro.Desplazamiento)
            .Take(filtro.Limite)
            .ToList();
    }

    // SQL Server no distingue mayúsculas y SQLite sí: se compara en minúscula para responder igual en los dos (D-31).
    public bool ExistePorCodigo(string codigo)
    {
        var codigoBuscado = codigo.ToLower();
        return contexto.Torneos.Any(t => t.Codigo.ToLower() == codigoBuscado);
    }

    public void Agregar(Torneo torneo)
    {
        contexto.Torneos.Add(torneo);
        Guardar(torneo);
    }

    public void Actualizar(Torneo torneo)
    {
        contexto.Torneos.Update(torneo);
        Guardar(torneo);
    }

    public bool ExisteVigenteDelOrganizador(int organizadorId)
    {
        _ = organizadorId;
        throw new NotImplementedException();
    }

    // Otro torneo puede ocupar el código entre la validación del servicio y el guardado (D-23).
    private void Guardar(Torneo torneo)
    {
        try
        {
            contexto.SaveChanges();
        }
        catch(DbUpdateException)
        {
            // Se suelta el torneo que falló para que el contexto se pueda seguir usando.
            contexto.Entry(torneo).State = EntityState.Detached;
            if(ExisteOtroTorneoConElCodigo(torneo))
            {
                throw new CodigoTorneoYaExisteException();
            }

            throw;
        }
    }

    private bool ExisteOtroTorneoConElCodigo(Torneo torneo)
    {
        var codigo = torneo.Codigo.ToLower();
        return contexto.Torneos.Any(t => t.Codigo.ToLower() == codigo && t.Id != torneo.Id);
    }
}
