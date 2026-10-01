using FarmeORT.Excepciones;

namespace FarmeORT.Servicios;

// Los DTOs declaran opcionales los números y fechas para detectar cuándo se omiten. El DTO ya los exige;
// el servicio los vuelve a controlar porque no depende de quién lo invoque.
internal static class DatosObligatorios
{
    public static T Obtener<T>(T? valor, string mensaje)
        where T : struct
    {
        if(valor == null)
        {
            throw new DatosInvalidosException(mensaje);
        }

        return valor.Value;
    }
}
