using System.Security.Cryptography;
using FarmeORT.IServicios.Seguridad;

namespace FarmeORT.Servicios.Seguridad;

internal sealed class HasheadorContrasenas : IHasheadorContrasenas
{
    private const int TamanoSal = 16;
    private const int TamanoHash = 32;
    private const int Iteraciones = 100_000;
    private const char Separador = '.';

    private static readonly HashAlgorithmName Algoritmo = HashAlgorithmName.SHA256;

    public string Hashear(string contrasena)
    {
        var sal = RandomNumberGenerator.GetBytes(TamanoSal);
        var hash = Derivar(contrasena, sal);
        return $"{Convert.ToBase64String(sal)}{Separador}{Convert.ToBase64String(hash)}";
    }

    public bool Verificar(string contrasena, string hash)
    {
        var partes = hash.Split(Separador);
        if(partes.Length != 2)
        {
            return false;
        }

        byte[] sal;
        byte[] hashEsperado;
        try
        {
            sal = Convert.FromBase64String(partes[0]);
            hashEsperado = Convert.FromBase64String(partes[1]);
        }
        catch(FormatException)
        {
            // Un hash guardado con otro formato no se puede verificar: cuenta como contraseña incorrecta.
            return false;
        }

        var hashCalculado = Derivar(contrasena, sal);
        return CryptographicOperations.FixedTimeEquals(hashCalculado, hashEsperado);
    }

    private static byte[] Derivar(string contrasena, byte[] sal)
    {
        return Rfc2898DeriveBytes.Pbkdf2(contrasena, sal, Iteraciones, Algoritmo, TamanoHash);
    }
}
