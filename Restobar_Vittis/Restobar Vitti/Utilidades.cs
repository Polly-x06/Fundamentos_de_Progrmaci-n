using System;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace SistemaVittis;

public static class Utilidades
{
    public static bool EsSoloNumeros(string texto)
    {
        if (string.IsNullOrEmpty(texto)) return false;

        foreach (char c in texto)
            if (!char.IsDigit(c)) return false;

        return true;
    }

    public static string FormatearMoneda(double monto) => $"S/ {monto:F2}";

    public static string? LimpiarTexto(string? texto)
    {
        if (texto == null) return null;
        string limpio = texto.Trim();
        return limpio.Length == 0 ? null : limpio;
    }

    public static double CalcularPorcentaje(int parte, int total)
    {
        if (total <= 0) return 0;
        return (parte * 100.0) / total;
    }

    /// <summary>
    /// Calcula el hash SHA-256 de un texto y lo devuelve en formato
    /// hexadecimal (64 caracteres). Se usa para no guardar contraseñas
    /// en texto plano dentro de usuarios.json.
    /// </summary>
    public static string CalcularHashSha256(string texto)
    {
        byte[] bytesTexto = Encoding.UTF8.GetBytes(texto ?? "");
        byte[] bytesHash = SHA256.HashData(bytesTexto);

        var constructor = new StringBuilder(bytesHash.Length * 2);
        foreach (byte b in bytesHash)
            constructor.Append(b.ToString("x2"));

        return constructor.ToString();
    }

    /// <summary>
    /// Indica si un texto tiene la forma de un hash SHA-256 (64 caracteres
    /// hexadecimales). Sirve para distinguir contraseñas ya protegidas de
    /// contraseñas antiguas guardadas en texto plano, y así migrarlas.
    /// </summary>
    public static bool TextoEsHashSha256(string? texto)
    {
        if (string.IsNullOrEmpty(texto) || texto.Length != 64) return false;

        foreach (char c in texto)
            if (!Uri.IsHexDigit(c)) return false;

        return true;
    }

    /// <summary>
    /// Busca el logo de Vittis en las carpetas habituales de publicación
    /// y lo devuelve como imagen lista para usar en un PictureBox o como
    /// ícono de ventana. Si no se encuentra o el archivo está dañado,
    /// devuelve null en vez de lanzar una excepción (para no cerrar la
    /// aplicación solo porque falte una imagen).
    /// </summary>
    public static Image? CargarLogoVittis()
    {
        string[] rutasPosibles =
        {
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logo_vittis.png"),
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "logo_vittis.png"),
            Path.Combine(Directory.GetCurrentDirectory(), "logo_vittis.png"),
            "logo_vittis.png"
        };

        foreach (string ruta in rutasPosibles)
        {
            if (!File.Exists(ruta)) continue;

            try
            {
                return Image.FromFile(ruta);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Advertencia: no se pudo cargar el logo desde '{ruta}' ({ex.Message}).");
            }
        }

        return null;
    }
}
