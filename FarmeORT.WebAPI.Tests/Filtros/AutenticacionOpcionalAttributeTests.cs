using FarmeORT.IServicios.Sesiones;
using FarmeORT.WebAPI.Filtros;
using Moq;

namespace FarmeORT.WebAPI.Tests.Filtros;

[TestClass]
public sealed class AutenticacionOpcionalAttributeTests
{
    private Mock<IServicioSesiones> _servicioSesiones = null!;
    private AutenticacionOpcionalAttribute _filtro = null!;

    [TestInitialize]
    public void Inicializar()
    {
        _servicioSesiones = new Mock<IServicioSesiones>(MockBehavior.Strict);
        _filtro = new AutenticacionOpcionalAttribute();
    }

    [TestMethod]
    public void OnAuthorization_SinHeaderAuthorization_SigueComoAnonimoSinGuardarSesion()
    {
        var contexto = ContextoDeFiltro.Crear(_servicioSesiones.Object);

        _filtro.OnAuthorization(contexto);

        Assert.IsNull(contexto.Result);
        Assert.IsFalse(contexto.HttpContext.Items.ContainsKey(ClavesContexto.SesionAutenticada));
    }
}
