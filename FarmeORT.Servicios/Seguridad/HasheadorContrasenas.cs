using FarmeORT.IServicios.Seguridad;

namespace FarmeORT.Servicios.Seguridad;

internal sealed class HasheadorContrasenas : IHasheadorContrasenas
{
    public string Hashear(string contrasena)
    {
        throw new NotImplementedException();
    }

    public bool Verificar(string contrasena, string hash)
    {
        throw new NotImplementedException();
    }
}
