using System;
using System.Drawing;
using System.Windows.Forms;

namespace SistemaVittis;

public class FormLogin : Form
{
    private readonly GestorUsuarios gestorUsuarios = new();

    private readonly Color ColorFondo = Color.FromArgb(10, 30, 70);
    private readonly Color ColorTarjeta = Color.FromArgb(15, 40, 90);
    private readonly Color ColorAcento = Color.FromArgb(240, 170, 20);
    private readonly Color ColorTexto = Color.FromArgb(255, 255, 255);
    private readonly Color ColorTextoSuave = Color.FromArgb(160, 185, 220);
    private readonly Color ColorCampo = Color.FromArgb(20, 55, 115);
    private readonly Color ColorCampoHover = Color.FromArgb(30, 70, 140);
    private readonly Color ColorPeligro = Color.FromArgb(220, 90, 90);

    private TextBox txtUsuario;
    private TextBox txtContrasena;
    private CheckBox chkMostrarContrasena;
    private Label lblError;
    private Button btnIngresar;

    /// <summary>Nombre del usuario que inició sesión correctamente.</summary>
    public string UsuarioActual { get; private set; } = "";

    public FormLogin()
    {
        InicializarComponentes();
    }

    private void InicializarComponentes()
    {
        Text = "Iniciar sesión — Vittis Restobar";
        Size = new Size(440, 660);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = ColorFondo;
        ForeColor = ColorTexto;
        Font = new Font("Segoe UI", 10f);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        var panelTarjeta = new Panel
        {
            Location = new Point(40, 40),
            Size = new Size(360, 540),
            BackColor = ColorTarjeta
        };
        Controls.Add(panelTarjeta);

        var franjaSuperior = new Panel
        {
            Dock = DockStyle.Top,
            Height = 3,
            BackColor = ColorAcento
        };
        panelTarjeta.Controls.Add(franjaSuperior);

        int innerX = 30, innerAncho = 300;
        int y = 26;

        var picLogo = new PictureBox
        {
            Location = new Point(innerX + (innerAncho - 140) / 2, y),
            Size = new Size(140, 100),
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = ColorTarjeta
        };

        Image? logo = Utilidades.CargarLogoVittis();
        if (logo != null)
        {
            picLogo.Image = logo;
            if (logo is Bitmap logoBitmap)
            {
                try { Icon = Icon.FromHandle(logoBitmap.GetHicon()); }
                catch { /* si no se puede crear el ícono, se sigue con el ícono por defecto */ }
            }
        }

        panelTarjeta.Controls.Add(picLogo);
        y += 100 + 18;

        var lblTitulo = new Label
        {
            Text = "Vittis Restobar",
            Location = new Point(innerX, y),
            Size = new Size(innerAncho, 30),
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 15f, FontStyle.Bold),
            ForeColor = ColorAcento
        };
        panelTarjeta.Controls.Add(lblTitulo);
        y += 30 + 2;

        var lblSubtitulo = new Label
        {
            Text = "Inicia sesión para continuar",
            Location = new Point(innerX, y),
            Size = new Size(innerAncho, 20),
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 9.5f),
            ForeColor = ColorTextoSuave
        };
        panelTarjeta.Controls.Add(lblSubtitulo);
        y += 20 + 22;

        var lblUsuario = new Label
        {
            Text = "Usuario",
            Location = new Point(innerX, y),
            Size = new Size(innerAncho, 18),
            ForeColor = ColorTextoSuave
        };
        panelTarjeta.Controls.Add(lblUsuario);
        y += 18 + 4;

        txtUsuario = new TextBox
        {
            Location = new Point(innerX, y),
            Size = new Size(innerAncho, 30),
            BackColor = ColorCampo,
            ForeColor = ColorTexto,
            BorderStyle = BorderStyle.FixedSingle,
            PlaceholderText = "Ej: Carlos Paredes"
        };
        txtUsuario.Enter += (_, _) => txtUsuario.BackColor = ColorCampoHover;
        txtUsuario.Leave += (_, _) => txtUsuario.BackColor = ColorCampo;
        panelTarjeta.Controls.Add(txtUsuario);
        y += 30 + 18;

        var lblContrasena = new Label
        {
            Text = "Contraseña",
            Location = new Point(innerX, y),
            Size = new Size(innerAncho, 18),
            ForeColor = ColorTextoSuave
        };
        panelTarjeta.Controls.Add(lblContrasena);
        y += 18 + 4;

        txtContrasena = new TextBox
        {
            Location = new Point(innerX, y),
            Size = new Size(innerAncho, 30),
            BackColor = ColorCampo,
            ForeColor = ColorTexto,
            BorderStyle = BorderStyle.FixedSingle,
            UseSystemPasswordChar = true
        };
        txtContrasena.Enter += (_, _) => txtContrasena.BackColor = ColorCampoHover;
        txtContrasena.Leave += (_, _) => txtContrasena.BackColor = ColorCampo;
        panelTarjeta.Controls.Add(txtContrasena);
        y += 30 + 10;

        chkMostrarContrasena = new CheckBox
        {
            Text = "Mostrar contraseña",
            Location = new Point(innerX, y),
            AutoSize = true,
            ForeColor = ColorTextoSuave,
            Cursor = Cursors.Hand
        };
        chkMostrarContrasena.CheckedChanged += (_, _) =>
            txtContrasena.UseSystemPasswordChar = !chkMostrarContrasena.Checked;
        panelTarjeta.Controls.Add(chkMostrarContrasena);
        y += 24 + 12;

        lblError = new Label
        {
            Text = "",
            Location = new Point(innerX, y),
            Size = new Size(innerAncho, 36),
            ForeColor = ColorPeligro,
            Font = new Font("Segoe UI", 9f)
        };
        panelTarjeta.Controls.Add(lblError);
        y += 36 + 6;

        btnIngresar = new Button
        {
            Text = "Ingresar",
            Location = new Point(innerX, y),
            Size = new Size(innerAncho, 44),
            FlatStyle = FlatStyle.Flat,
            BackColor = ColorAcento,
            ForeColor = Color.FromArgb(10, 30, 70),
            Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        btnIngresar.FlatAppearance.BorderSize = 0;
        btnIngresar.FlatAppearance.MouseOverBackColor = Color.FromArgb(255, 190, 60);
        btnIngresar.Click += BtnIngresar_Click;
        panelTarjeta.Controls.Add(btnIngresar);
        y += 44 + 24;

        var lblFooter = new Label
        {
            Text = "© Vittis Restobar",
            Location = new Point(innerX, y),
            Size = new Size(innerAncho, 18),
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 8f),
            ForeColor = ColorTextoSuave
        };
        panelTarjeta.Controls.Add(lblFooter);

        AcceptButton = btnIngresar;
        txtUsuario.Focus();
    }

    private void BtnIngresar_Click(object sender, EventArgs e)
    {
        string usuario = txtUsuario.Text.Trim();
        string contrasena = txtContrasena.Text;

        if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(contrasena))
        {
            MostrarError("Ingresa tu usuario y contraseña.");
            return;
        }

        if (gestorUsuarios.ValidarCredenciales(usuario, contrasena))
        {
            UsuarioActual = usuario;
            DialogResult = DialogResult.OK;
            Close();
        }
        else
        {
            MostrarError("Usuario o contraseña incorrectos.");
            txtContrasena.Clear();
            txtContrasena.Focus();
        }
    }

    private void MostrarError(string mensaje)
    {
        lblError.Text = mensaje;
    }
}
