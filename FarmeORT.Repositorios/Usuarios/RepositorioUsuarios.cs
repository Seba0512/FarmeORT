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

    public IReadOnlyList<Usuario> Listar(FiltroUsuarios filtro)
    {
        IQueryable<Usuario> consulta = contexto.Usuarios;
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
