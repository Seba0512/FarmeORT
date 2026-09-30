using FarmeORT.Excepciones;

namespace FarmeORT.Dominio.Usuarios;

public static class ReglaContrasena
{
    private const int LargoMinimo = 15;
    private const int LargoMaximo = 25;

    public static void Validar(string contrasena)
    {
        if(contrasena.Length is < LargoMinimo or > LargoMaximo)
        {
            throw new DatosInvalidosException("La contraseña debe tener entre 15 y 25 caracteres.");
        }

        var tieneMayuscula = contrasena.Any(char.IsAsciiLetterUpper);
        var tieneMinuscula = contrasena.Any(char.IsAsciiLetterLower);
        var tieneNumero = contrasena.Any(char.IsAsciiDigit);
        var tieneSimbolo = contrasena.Any(caracter => !char.IsAsciiLetterOrDigit(caracter));
        if(!(tieneMayuscula && tieneMinuscula && tieneNumero && tieneSimbolo))
        {
            throw new DatosInvalidosException(
                "La contraseña debe tener al menos una mayúscula, una minúscula, un número y un símbolo.");
        }
    }
}
