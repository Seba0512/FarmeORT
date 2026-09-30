using FarmeORT.Dominio.Sesiones;
using FarmeORT.Dominio.Usuarios;
using FarmeORT.Repositorios.Sesiones;

namespace FarmeORT.Repositorios.Tests.Sesiones;

[TestClass]
public sealed class RepositorioSesionesTests
{
    private static readonly Guid Token = Guid.Parse("3f2b8c1e-5d4a-4e7b-9c6f-1a2b3c4d5e6f");

    private BaseDeDatosEnMemoria _baseDeDatos = null!;
    private RepositorioSesiones _repositorio = null!;

    [TestInitialize]
    public void Inicializar()
    {
        _baseDeDatos = new BaseDeDatosEnMemoria();
        _repositorio = new RepositorioSesiones(_baseDeDatos.Contexto);
    }

    [TestCleanup]
    public void Limpiar()
    {
        _baseDeDatos.Dispose();
    }

    [TestMethod]
    public void Agregar_SesionNueva_QuedaGuardadaConSuUsuario()
    {
        var usuario = new Usuario { Email = "jugador@farmeort.com", Rol = Rol.Jugador, ContrasenaHash = "hash-jugador" };
        _baseDeDatos.Contexto.Usuarios.Add(usuario);
        _baseDeDatos.Contexto.SaveChanges();

        _repositorio.Agregar(new Sesion(Token, usuario));
        _baseDeDatos.Contexto.ChangeTracker.Clear();

        var guardada = _baseDeDatos.Contexto.Sesiones.SingleOrDefault(s => s.Token == Token);
        Assert.IsNotNull(guardada);
        Assert.AreEqual(usuario.Id, guardada.UsuarioId);
    }
}
