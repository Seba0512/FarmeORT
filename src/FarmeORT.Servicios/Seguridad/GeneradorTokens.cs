using FarmeORT.IServicios.Seguridad;

namespace FarmeORT.Servicios.Seguridad;

internal sealed class GeneradorTokens : IGeneradorTokens
{
    public Guid Generar()
    {
        return Guid.NewGuid();
    }
}
