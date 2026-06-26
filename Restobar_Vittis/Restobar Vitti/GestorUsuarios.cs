using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace SistemaVittis;

public class GestorUsuarios
{
    private readonly string rutaArchivo =
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "usuarios.json");
    private List<Usuario> usuarios = new();

    public GestorUsuarios()
    {
        CargarUsuarios();
    }

    private void CargarUsuarios()
    {
        if (!File.Exists(rutaArchivo))
        {
            usuarios = new List<Usuario>
            {
                new Usuario("Carlos Paredes", Utilidades.CalcularHashSha256("admin123"))
            };
            GuardarUsuarios();
            return;
        }

        try
        {
            string json = File.ReadAllText(rutaArchivo);
            usuarios = JsonSerializer.Deserialize<List<Usuario>>(json) ?? new List<Usuario>();

            if (usuarios.Count == 0)
                usuarios.Add(new Usuario("Carlos Paredes", Utilidades.CalcularHashSha256("admin123")));

            MigrarContrasenasSinHash();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Advertencia: no se pudo leer usuarios.json ({ex.Message}). Se usará el usuario por defecto.");
            usuarios = new List<Usuario> { new Usuario("Carlos Paredes", Utilidades.CalcularHashSha256("admin123")) };
        }
    }

    /// <summary>
    /// Convierte al hash SHA-256 las contraseñas que todavía estén
    /// guardadas en texto plano (por ejemplo, si el archivo usuarios.json
    /// viene de una versión anterior del sistema). Así se deja de
    /// almacenar contraseñas legibles sin afectar a los usuarios ya
    /// creados, que pueden seguir ingresando con su misma contraseña.
    /// </summary>
    private void MigrarContrasenasSinHash()
    {
        bool huboCambios = false;

        foreach (var u in usuarios)
        {
            if (!Utilidades.TextoEsHashSha256(u.Contrasena))
            {
                u.Contrasena = Utilidades.CalcularHashSha256(u.Contrasena);
                huboCambios = true;
            }
        }

        if (huboCambios) GuardarUsuarios();
    }

    private void GuardarUsuarios()
    {
        try
        {
            var opciones = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(usuarios, opciones);
            File.WriteAllText(rutaArchivo, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Advertencia: no se pudo guardar usuarios.json ({ex.Message}).");
        }
    }

    public bool ValidarCredenciales(string nombreUsuario, string contrasena)
    {
        if (string.IsNullOrWhiteSpace(nombreUsuario) || string.IsNullOrWhiteSpace(contrasena))
            return false;

        string usuarioIngresado = nombreUsuario.Trim();
        string hashIngresado = Utilidades.CalcularHashSha256(contrasena);

        foreach (var u in usuarios)
        {
            bool coincideUsuario = string.Equals(u.NombreUsuario, usuarioIngresado,
                StringComparison.OrdinalIgnoreCase);
            bool coincideContrasena = u.Contrasena == hashIngresado;

            if (coincideUsuario && coincideContrasena)
                return true;
        }

        return false;
    }

    public bool ExisteUsuario(string nombreUsuario)
    {
        if (string.IsNullOrWhiteSpace(nombreUsuario)) return false;
        string buscado = nombreUsuario.Trim();
        foreach (var u in usuarios)
            if (string.Equals(u.NombreUsuario, buscado, StringComparison.OrdinalIgnoreCase))
                return true;
        return false;
    }

    public bool AgregarUsuario(string nombreUsuario, string contrasena)
    {
        if (string.IsNullOrWhiteSpace(nombreUsuario) || string.IsNullOrWhiteSpace(contrasena))
            return false;

        if (ExisteUsuario(nombreUsuario))
            return false;

        usuarios.Add(new Usuario(nombreUsuario.Trim(), Utilidades.CalcularHashSha256(contrasena)));
        GuardarUsuarios();
        return true;
    }

    public List<string> ObtenerNombresUsuarios()
    {
        var nombres = new List<string>();
        foreach (var u in usuarios) nombres.Add(u.NombreUsuario);
        return nombres;
    }
}
