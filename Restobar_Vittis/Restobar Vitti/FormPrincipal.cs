using System;
using System.Drawing;
using System.Windows.Forms;

namespace SistemaVittis;

public class FormPrincipal : Form
{
    private readonly GestorClientes gestor = new();
    private readonly GestorUsuarios gestorUsuarios = new();
    private readonly GestorInventario gestorInventario = new();
    private readonly string usuarioActual;

    
    private readonly Color ColorFondo = Color.FromArgb(10, 30, 70);   
    private readonly Color ColorSidebar = Color.FromArgb(15, 40, 90);  
    private readonly Color ColorAcento = Color.FromArgb(240, 170, 20);  
    private readonly Color ColorAcentoAlt = Color.FromArgb(255, 140, 0);  
    private readonly Color ColorTexto = Color.FromArgb(255, 255, 255);  
    private readonly Color ColorTextoSuave = Color.FromArgb(160, 185, 220);  
    private readonly Color ColorBoton = Color.FromArgb(20, 55, 115);   
    private readonly Color ColorBotonHover = Color.FromArgb(30, 70, 140);   
    private readonly Color ColorPeligro = Color.FromArgb(200, 50, 50);  

    private Panel panelContenido;

    /// <summary>
    /// Indica si el usuario pidió cerrar sesión (en vez de cerrar la
    /// aplicación por completo). Program.cs revisa esta propiedad para
    /// decidir si debe volver a mostrar la pantalla de inicio de sesión.
    /// </summary>
    public bool DeseaCerrarSesion { get; private set; } = false;

    public FormPrincipal(string usuarioActual = "")
    {
        this.usuarioActual = string.IsNullOrWhiteSpace(usuarioActual) ? "Invitado" : usuarioActual;
        InicializarComponentes();
        gestor.CargarDatosAlIniciar();
        MostrarBienvenida();
    }

    private void InicializarComponentes()
    {
        Text = "Vittis Restobar";
        Size = new Size(960, 620);
        MinimumSize = new Size(860, 560);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = ColorFondo;
        ForeColor = ColorTexto;
        Font = new Font("Segoe UI", 10f);
        var panelSidebar = new Panel
        {
            Dock = DockStyle.Left,
            Width = 230,
            BackColor = ColorSidebar
        };

        var picLogo = new PictureBox
        {
            Dock = DockStyle.Top,
            Height = 140,
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = ColorSidebar,
            Padding = new Padding(18, 12, 18, 8)
        };

        Image? logo = Utilidades.CargarLogoVittis();
        if (logo != null) picLogo.Image = logo;

        var separador = new Panel
        {
            Dock = DockStyle.Top,
            Height = 2,
            BackColor = ColorAcento
        };

        var btnRegistrar = CrearBotonMenu("➕   Registrar Ingreso");
        var btnHistorial = CrearBotonMenu("📋   Ver Historial");
        var btnOrdenar = CrearBotonMenu("🔤   Ordenar por Nombre");
        var btnReporte = CrearBotonMenu("📊   Reporte Diario");
        var btnInventario = CrearBotonMenu("📦   Inventario");
        var btnUsuarios = CrearBotonMenu("👤   Usuarios");
        var btnEliminarTodo = CrearBotonMenu("🗑   Eliminar Historial");
        var btnCerrarSesion = CrearBotonMenu("🚪   Cerrar Sesión");

        btnRegistrar.Click += (_, _) => { using var f = new FormRegistro(gestor, ObtenerEstilos(), usuarioActual); f.ShowDialog(this); };
        btnHistorial.Click += (_, _) => { using var f = new FormHistorial(gestor, ObtenerEstilos()); f.ShowDialog(this); };
        btnOrdenar.Click += (_, _) => EjecutarOrdenar();
        btnReporte.Click += (_, _) => { using var f = new FormReporte(gestor, ObtenerEstilos()); f.ShowDialog(this); };
        btnInventario.Click += (_, _) => { using var f = new FormInventario(gestorInventario, ObtenerEstilos(), usuarioActual, gestor); f.ShowDialog(this); };
        btnUsuarios.Click += (_, _) => { using var f = new FormUsuarios(gestorUsuarios, ObtenerEstilos()); f.ShowDialog(this); };
        btnEliminarTodo.Click += (_, _) => EjecutarEliminarTodo();
        btnCerrarSesion.Click += (_, _) => EjecutarCerrarSesion();

        var panelBotones = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12, 16, 12, 0) };
        panelBotones.Controls.Add(btnCerrarSesion);
        panelBotones.Controls.Add(btnEliminarTodo);
        panelBotones.Controls.Add(btnUsuarios);
        panelBotones.Controls.Add(btnInventario);
        panelBotones.Controls.Add(btnReporte);
        panelBotones.Controls.Add(btnOrdenar);
        panelBotones.Controls.Add(btnHistorial);
        panelBotones.Controls.Add(btnRegistrar);

        panelSidebar.Controls.Add(panelBotones);
        panelSidebar.Controls.Add(separador);
        panelSidebar.Controls.Add(picLogo);

        panelContenido = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = ColorFondo,
            Padding = new Padding(40)
        };

        Controls.Add(panelContenido);
        Controls.Add(panelSidebar);
    }

    private void MostrarBienvenida()
    {
        panelContenido.Controls.Clear();

        panelContenido.Controls.Add(new Label
        {
            Text = "Bienvenido al sistema",
            Font = new Font("Segoe UI", 22f, FontStyle.Bold),
            ForeColor = ColorAcento,
            AutoSize = true,
            Location = new Point(40, 80)
        });

        panelContenido.Controls.Add(new Label
        {
            Text = "Selecciona una opción del menú para comenzar.",
            Font = new Font("Segoe UI", 11f),
            ForeColor = ColorTextoSuave,
            AutoSize = true,
            Location = new Point(42, 130)
        });

        panelContenido.Controls.Add(new Label
        {
            Text = $"Sesión iniciada como: {usuarioActual}",
            Font = new Font("Segoe UI", 10f, FontStyle.Bold),
            ForeColor = ColorTexto,
            AutoSize = true,
            Location = new Point(42, 166)
        });
    }

    private void EjecutarOrdenar()
    {
        gestor.OrdenarClientesPorNombre();
        MessageBox.Show("Clientes ordenados alfabéticamente.", "Listo",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void EjecutarEliminarTodo()
    {
        var confirmacion = MessageBox.Show(
            "¿Estás seguro de que deseas eliminar TODO el historial de clientes?\r\n" +
            "Esta acción borrará todos los registros permanentemente y\r\n" +
            "vaciará el archivo clientes.json.",
            "Advertencia crítica",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Error);

        if (confirmacion != DialogResult.Yes) return;

        // Segunda confirmación para evitar borrados accidentales.
        var dobleConfirmacion = MessageBox.Show(
            "Esta es tu última oportunidad.\r\n¿Confirmas que deseas eliminar TODOS los clientes?",
            "Confirmar eliminación total",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (dobleConfirmacion != DialogResult.Yes) return;

        // EliminarTodo() pone totalClientes en 0, limpia el array y guarda
        // el JSON con una lista vacía ([]). Los clientes desaparecen del
        // archivo inmediatamente.
        gestor.EliminarTodo();

        MessageBox.Show("Se eliminó todo el historial de clientes.\r\nEl archivo clientes.json ha sido vaciado.", "Listo",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void EjecutarCerrarSesion()
    {
        var confirmacion = MessageBox.Show(
            "¿Deseas cerrar la sesión actual y volver a la pantalla de inicio?",
            "Cerrar sesión",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (confirmacion != DialogResult.Yes) return;

        DeseaCerrarSesion = true;
        Close();
    }

    private Button CrearBotonMenu(string texto)
    {
        var btn = new Button
        {
            Text = texto,
            Dock = DockStyle.Top,
            Height = 50,
            FlatStyle = FlatStyle.Flat,
            BackColor = ColorBoton,
            ForeColor = ColorTexto,
            Font = new Font("Segoe UI", 10f),
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(12, 0, 0, 0),
            Cursor = Cursors.Hand,
            Margin = new Padding(0, 0, 0, 4)
        };
        btn.FlatAppearance.BorderSize = 0;
        btn.FlatAppearance.MouseOverBackColor = ColorBotonHover;
        btn.FlatAppearance.MouseDownBackColor = ColorAcento;
        return btn;
    }

    public EstilosVittis ObtenerEstilos() => new()
    {
        ColorFondo = ColorFondo,
        ColorSidebar = ColorSidebar,
        ColorAcento = ColorAcento,
        ColorAcentoAlt = ColorAcentoAlt,
        ColorTexto = ColorTexto,
        ColorTextoSuave = ColorTextoSuave,
        ColorBoton = ColorBoton,
        ColorBotonHover = ColorBotonHover,
        ColorPeligro = ColorPeligro
    };
}