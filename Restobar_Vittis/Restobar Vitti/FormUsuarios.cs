using System;
using System.Drawing;
using System.Windows.Forms;

namespace SistemaVittis;

/// <summary>
/// Permite crear nuevos usuarios del sistema. Solo es accesible desde
/// dentro de la aplicación (FormPrincipal), es decir, estando ya
/// logueado; nunca se ofrece esta opción desde la pantalla de login.
/// </summary>
public class FormUsuarios : Form
{
    /// <summary>Cantidad mínima de caracteres exigida para una contraseña nueva.</summary>
    private const int LongitudMinimaContrasena = 4;

    private readonly GestorUsuarios gestorUsuarios;
    private readonly EstilosVittis estilos;

    private TextBox txtNuevoUsuario;
    private TextBox txtNuevaContrasena;
    private CheckBox chkMostrarContrasena;
    private ListBox lstUsuarios;
    private Label lblMensaje;

    public FormUsuarios(GestorUsuarios gestorUsuarios, EstilosVittis estilos)
    {
        this.gestorUsuarios = gestorUsuarios;
        this.estilos = estilos;
        InicializarComponentes();
        CargarUsuarios();
    }

    private void InicializarComponentes()
    {
        Text = "Gestión de usuarios — Vittis";
        Size = new Size(480, 600);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = estilos.ColorFondo;
        ForeColor = estilos.ColorTexto;
        Font = new Font("Segoe UI", 10f);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;

        int x = 30, y = 24, ancho = 390;

        Controls.Add(new Label
        {
            Text = "Crear nuevo usuario",
            Font = new Font("Segoe UI", 14f, FontStyle.Bold),
            ForeColor = estilos.ColorAcento,
            Location = new Point(x, y),
            AutoSize = true
        });
        y += 46;

        Controls.Add(new Label { Text = "Nombre de usuario", Location = new Point(x, y), AutoSize = true, ForeColor = estilos.ColorTextoSuave });
        y += 22;
        txtNuevoUsuario = new TextBox
        {
            Location = new Point(x, y),
            Size = new Size(ancho, 28),
            BackColor = estilos.ColorBoton,
            ForeColor = estilos.ColorTexto,
            BorderStyle = BorderStyle.FixedSingle,
            PlaceholderText = "Ej: Maria Lopez"
        };
        // Validación: no se permiten dígitos en el nombre de usuario.
        // Solo letras, espacios y teclas de control (Backspace, Delete, etc.).
        txtNuevoUsuario.KeyPress += (_, e) =>
        {
            if (char.IsDigit(e.KeyChar))
                e.Handled = true;
        };
        Controls.Add(txtNuevoUsuario);
        y += 46;

        Controls.Add(new Label
        {
            Text = $"Contraseña (mínimo {LongitudMinimaContrasena} caracteres)",
            Location = new Point(x, y),
            AutoSize = true,
            ForeColor = estilos.ColorTextoSuave
        });
        y += 22;
        txtNuevaContrasena = new TextBox
        {
            Location = new Point(x, y),
            Size = new Size(ancho, 28),
            BackColor = estilos.ColorBoton,
            ForeColor = estilos.ColorTexto,
            BorderStyle = BorderStyle.FixedSingle,
            UseSystemPasswordChar = true
        };
        Controls.Add(txtNuevaContrasena);
        y += 34;

        chkMostrarContrasena = new CheckBox
        {
            Text = "Mostrar contraseña",
            Location = new Point(x, y),
            AutoSize = true,
            ForeColor = estilos.ColorTextoSuave,
            Cursor = Cursors.Hand
        };
        chkMostrarContrasena.CheckedChanged += (_, _) =>
            txtNuevaContrasena.UseSystemPasswordChar = !chkMostrarContrasena.Checked;
        Controls.Add(chkMostrarContrasena);
        y += 30;

        lblMensaje = new Label
        {
            Text = "",
            Location = new Point(x, y),
            Size = new Size(ancho, 36),
            ForeColor = estilos.ColorPeligro,
            Font = new Font("Segoe UI", 9f)
        };
        Controls.Add(lblMensaje);
        y += 36;

        var btnCrear = new Button
        {
            Text = "Crear usuario",
            Location = new Point(x, y),
            Size = new Size(ancho, 42),
            FlatStyle = FlatStyle.Flat,
            BackColor = estilos.ColorAcento,
            ForeColor = Color.FromArgb(10, 30, 70),
            Font = new Font("Segoe UI", 10f, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        btnCrear.FlatAppearance.BorderSize = 0;
        btnCrear.Click += BtnCrear_Click;
        Controls.Add(btnCrear);
        y += 58;

        Controls.Add(new Label
        {
            Text = "Usuarios registrados",
            Font = new Font("Segoe UI", 11.5f, FontStyle.Bold),
            ForeColor = estilos.ColorAcento,
            Location = new Point(x, y),
            AutoSize = true
        });
        y += 30;

        lstUsuarios = new ListBox
        {
            Location = new Point(x, y),
            Size = new Size(ancho, 150),
            BackColor = estilos.ColorBoton,
            ForeColor = estilos.ColorTexto,
            BorderStyle = BorderStyle.FixedSingle,
            Font = new Font("Segoe UI", 10f)
        };
        Controls.Add(lstUsuarios);

        AcceptButton = btnCrear;
    }

    private void CargarUsuarios()
    {
        lstUsuarios.Items.Clear();
        foreach (var nombre in gestorUsuarios.ObtenerNombresUsuarios())
            lstUsuarios.Items.Add(nombre);
    }

    private void BtnCrear_Click(object sender, EventArgs e)
    {
        string nombre = txtNuevoUsuario.Text.Trim();
        string contrasena = txtNuevaContrasena.Text;

        if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(contrasena))
        {
            MostrarMensaje("Ingresa un nombre de usuario y una contraseña.", esError: true);
            return;
        }

        if (contrasena.Length < LongitudMinimaContrasena)
        {
            MostrarMensaje($"La contraseña debe tener al menos {LongitudMinimaContrasena} caracteres.", esError: true);
            return;
        }

        if (gestorUsuarios.ExisteUsuario(nombre))
        {
            MostrarMensaje("Ya existe un usuario con ese nombre.", esError: true);
            return;
        }

        if (gestorUsuarios.AgregarUsuario(nombre, contrasena))
        {
            MostrarMensaje($"Usuario \"{nombre}\" creado correctamente.", esError: false);
            txtNuevoUsuario.Clear();
            txtNuevaContrasena.Clear();
            CargarUsuarios();
        }
        else
        {
            MostrarMensaje("No se pudo crear el usuario.", esError: true);
        }
    }

    private void MostrarMensaje(string texto, bool esError)
    {
        lblMensaje.Text = texto;
        lblMensaje.ForeColor = esError ? estilos.ColorPeligro : estilos.ColorAcento;
    }
}
