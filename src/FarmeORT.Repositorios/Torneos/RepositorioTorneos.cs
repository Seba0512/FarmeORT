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
        consulta = FiltrarPorEstado(consulta, filtro);
        consulta = FiltrarPorFechaDeInicio(consulta, filtro.FechaDesde, filtro.FechaHasta);
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
        return contexto.Torneos.Any(t => t.OrganizadorId == organizadorId
            && t.Estado != EstadoTorneo.Finalizado && t.Estado != EstadoTorneo.Cancelado);
    }

    // El anónimo no ve los Cancelados aunque los pida por estado (D-36).
    private static IQueryable<Torneo> FiltrarPorEstado(IQueryable<Torneo> consulta, FiltroTorneos filtro)
    {
        if(filtro.Estado != null)
        {
            consulta = consulta.Where(t => t.Estado == filtro.Estado.Value);
        }

        if(filtro.ExcluirCancelados)
        {
            consulta = consulta.Where(t => t.Estado != EstadoTorneo.Cancelado);
        }

        return consulta;
    }

    private static IQueryable<Torneo> FiltrarPorFechaDeInicio(
        IQueryable<Torneo> consulta,
        DateOnly? fechaDesde,
        DateOnly? fechaHasta)
    {
        if(fechaDesde != null)
        {
            consulta = consulta.Where(t => t.FechaInicio >= fechaDesde.Value);
        }

        if(fechaHasta != null)
        {
            consulta = consulta.Where(t => t.FechaInicio <= fechaHasta.Value);
        }

        return consulta;
    }

    // Otro torneo puede ocupar el código entre la validación del servicio y el guardado (D-23).
    private void Guardar(Torneo torneo)
    {
        try
        {
            contexto.SaveChanges();
        }
        catch(DbUpdateConcurrencyException excepcion)
        {
            throw TraducirConflictoDeConcurrencia(excepcion);
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

    // SaveChanges revierte todo; se distingue el torneo de los tokens de batalla/ELO existentes.
    private Exception TraducirConflictoDeConcurrencia(DbUpdateConcurrencyException excepcion)
    {
        var cambioElTorneo = excepcion.Entries.Any(e => e.Entity is Torneo);
        contexto.ChangeTracker.Clear();
        if(cambioElTorneo)
        {
            return new TorneoModificadoException();
        }

        return new BatallaModificadaException();
    }

    private bool ExisteOtroTorneoConElCodigo(Torneo torneo)
    {
        var codigo = torneo.Codigo.ToLower();
        return contexto.Torneos.Any(t => t.Codigo.ToLower() == codigo && t.Id != torneo.Id);
    }
}
