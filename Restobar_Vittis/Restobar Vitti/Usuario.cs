using System;

namespace SistemaVittis;

public class Usuario
{
    public string NombreUsuario { get; set; }
    public string Contrasena { get; set; }

    public Usuario() { }

    public Usuario(string nombreUsuario, string contrasena)
    {
        NombreUsuario = nombreUsuario;
        Contrasena = contrasena;
    }
}
