using FarmeORT.Dominio.Inscripciones;
using FarmeORT.Dominio.Usuarios;
using FarmeORT.Excepciones;
using FarmeORT.IRepositorios.Usuarios;
using Microsoft.EntityFrameworkCore;

namespace FarmeORT.Repositorios.Usuarios;

internal sealed class RepositorioUsuarios(FarmeORTContext contexto) : IRepositorioUsuarios
{
    public Usuario? ObtenerPorEmail(string email)
    {
        return contexto.Usuarios.FirstOrDefault(u => u.Email == email);
    }

    public Usuario? ObtenerPorId(int id)
    {
        return contexto.Usuarios.SingleOrDefault(u => u.Id == id && !u.Eliminado);
    }

    public Usuario? ObtenerConRepertorio(int id)
    {
        return contexto.Usuarios
            .Include(u => u.Repertorio)
            .ThenInclude(d => d.Skill)
            .SingleOrDefault(u => u.Id == id && !u.Eliminado);
    }

    public IReadOnlyList<Usuario> Listar(FiltroUsuarios filtro)
    {
        var consulta = contexto.Usuarios.Where(u => !u.Eliminado);
        if(!string.IsNullOrWhiteSpace(filtro.Nombre))
        {
            var nombre = filtro.Nombre.ToLower();
            consulta = consulta.Where(u => u.Nombre.ToLower().Contains(nombre));
        }

        if(!string.IsNullOrWhiteSpace(filtro.Apellido))
        {
            var apellido = filtro.Apellido.ToLower();
            consulta = consulta.Where(u => u.Apellido.ToLower().Contains(apellido));
        }

        if(!string.IsNullOrWhiteSpace(filtro.Nickname))
        {
            var nickname = filtro.Nickname.ToLower();
            consulta = consulta.Where(u => u.Nickname.ToLower().Contains(nickname));
        }

        return consulta
            .OrderBy(u => u.Id)
            .Skip(filtro.Desplazamiento)
            .Take(filtro.Limite)
            .ToList();
    }

    public bool ExistePorNickname(string nickname)
    {
        return contexto.Usuarios.Any(u => u.Nickname == nickname);
    }

    public void Agregar(Usuario usuario)
    {
        contexto.Usuarios.Add(usuario);
        Guardar(usuario);
    }

    public void Actualizar(Usuario usuario)
    {
        contexto.Usuarios.Update(usuario);
        Guardar(usuario);
    }

    // La baja y el cierre de sesiones se guardan juntos: un solo SaveChanges es una transacción.
    public void Eliminar(Usuario usuario)
    {
        contexto.Sesiones.RemoveRange(contexto.Sesiones.Where(s => s.UsuarioId == usuario.Id));
        contexto.Usuarios.Update(usuario);
        contexto.SaveChanges();
    }

    // El jugador viene de ObtenerConRepertorio, en seguimiento: EF detecta solas las skills agregadas y
    // quitadas. Update() no sirve, porque marcaría como modificada una skill nueva con su clave ya asignada.
    // Otro pedido puede declarar la misma skill entre la carga y el guardado: la clave compuesta lo frena (D-23).
    public void GuardarRepertorio(Usuario jugador)
    {
        try
        {
            contexto.SaveChanges();
        }
        catch(DbUpdateException)
        {
            // Se sueltan las skills que fallaron para que el contexto se pueda seguir usando.
            var agregadas = contexto.ChangeTracker.Entries<SkillDeclarada>()
                .Where(e => e.State == EntityState.Added)
                .Select(e => e.Entity)
                .ToList();
            foreach(var declarada in agregadas)
            {
                contexto.Entry(declarada).State = EntityState.Detached;
            }

            if(agregadas.Any(EstaGuardada))
            {
                throw new SkillYaDeclaradaException();
            }

            throw;
        }
    }

    public IReadOnlyList<Usuario> ListarRanking(int cantidad)
    {
        return OrdenarParaRanking(JugadoresActivos(), cantidad);
    }

    public IReadOnlyList<Usuario> ListarRankingDelTorneo(int torneoId, int cantidad)
    {
        var participantes = JugadoresActivos()
            .Where(u => contexto.Inscripciones.Any(i => i.JugadorId == u.Id && i.TorneoId == torneoId
                && (i.Estado == EstadoInscripcion.Aceptada || i.Estado == EstadoInscripcion.Eliminada)));
        return OrdenarParaRanking(participantes, cantidad);
    }

    // Los dos rankings ordenan igual: ELO de mayor a menor y, si empatan, id de menor a mayor (D-61).
    private static IReadOnlyList<Usuario> OrdenarParaRanking(IQueryable<Usuario> jugadores, int cantidad)
    {
        return jugadores
            .OrderByDescending(u => u.Elo)
            .ThenBy(u => u.Id)
            .Take(cantidad)
            .ToList();
    }

    private IQueryable<Usuario> JugadoresActivos()
    {
        return contexto.Usuarios.Where(u => u.Rol == Rol.Jugador && !u.Eliminado);
    }

    private bool EstaGuardada(SkillDeclarada declarada)
    {
        return contexto.Set<SkillDeclarada>().Any(d => d.JugadorId == declarada.JugadorId && d.SkillId == declarada.SkillId);
    }

    // Otro usuario puede ocupar el email o nickname entre la validación del servicio y el guardado (D-23).
    private void Guardar(Usuario usuario)
    {
        try
        {
            contexto.SaveChanges();
        }
        catch(DbUpdateException)
        {
            contexto.Entry(usuario).State = EntityState.Detached;
            if(contexto.Usuarios.Any(u => u.Email == usuario.Email && u.Id != usuario.Id))
            {
                throw new EmailYaExisteException();
            }

            if(contexto.Usuarios.Any(u => u.Nickname == usuario.Nickname && u.Id != usuario.Id))
            {
                throw new NicknameYaExisteException();
            }

            throw;
        }
    }
}
