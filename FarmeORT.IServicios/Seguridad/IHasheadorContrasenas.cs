namespace FarmeORT.IServicios.Seguridad;

public interface IHasheadorContrasenas
{
    bool Verificar(string contrasena, string hash);
}
