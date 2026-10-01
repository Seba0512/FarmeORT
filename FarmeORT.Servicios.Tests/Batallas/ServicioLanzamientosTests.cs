using FarmeORT.Dominio.Batallas;
using FarmeORT.DTOs.Batallas;
using FarmeORT.Excepciones;
using FarmeORT.IRepositorios.Batallas;
using FarmeORT.Servicios.Batallas;
using Moq;

namespace FarmeORT.Servicios.Tests.Batallas;

[TestClass]
public sealed class ServicioLanzamientosTests
{
    [TestMethod]
    public void Lanzar_BatallaInexistente_LanzaBatallaNoEncontrada()
    {
        var repositorio = new Mock<IRepositorioBatallas>(MockBehavior.Strict);
        repositorio.Setup(r => r.ObtenerPorId(99)).Returns((Batalla?)null);
        var servicio = new ServicioLanzamientos(repositorio.Object);

        Assert.ThrowsExactly<BatallaNoEncontradaException>(
            () => servicio.Lanzar(99, 7, new LanzamientoRequest { SkillId = 3 }));
    }
}
