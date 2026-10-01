using FarmeORT.Repositorios.Batallas;

namespace FarmeORT.Repositorios.Tests.Batallas;

[TestClass]
public sealed class RepositorioBatallasTests
{
    private BaseDeDatosEnMemoria _baseDeDatos = null!;
    private RepositorioBatallas _repositorio = null!;

    [TestInitialize]
    public void Inicializar()
    {
        _baseDeDatos = new BaseDeDatosEnMemoria();
        _repositorio = new RepositorioBatallas(_baseDeDatos.Contexto);
    }

    [TestCleanup]
    public void Limpiar()
    {
        _baseDeDatos.Dispose();
    }

    [TestMethod]
    public void ObtenerPorId_BatallaInexistente_DevuelveNull()
    {
        var resultado = _repositorio.ObtenerPorId(99);

        Assert.IsNull(resultado);
    }
}
