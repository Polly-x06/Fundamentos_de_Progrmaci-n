using System;
using System.Drawing;
using System.Windows.Forms;

namespace SistemaVittis;

public class FormRegistro : Form
{
    private readonly GestorClientes gestor;
    private readonly EstilosVittis estilos;
    private readonly string usuarioActual;

    private TextBox txtNombre;
    private TextBox txtTelefono;
    private NumericUpDown numPersonas;
    private RadioButton rbLibre;
    private RadioButton rbHora;
    private NumericUpDown numHoras;
    private Label lblHoras;
    private Label lblTotal;

    public FormRegistro(GestorClientes gestor, EstilosVittis estilos, string usuarioActual = "")
    {
        this.gestor = gestor;
        this.estilos = estilos;
        this.usuarioActual = usuarioActual ?? "";
        InicializarComponentes();
    }

    private void InicializarComponentes()
    {
        Text = "Registrar Ingreso — Vittis";
        Size = new Size(490, 570);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = estilos.ColorFondo;
        ForeColor = estilos.ColorTexto;
        Font = new Font("Segoe UI", 10f);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;

        int x = 40, y = 30, ancho = 380;

        AgregarLabel("Registrar Ingreso", x, y, ancho, 30,
            new Font("Segoe UI", 14f, FontStyle.Bold), estilos.ColorAcento);
        y += 50;

        AgregarLabel("Nombre completo", x, y, ancho, 20);
        y += 22;
        txtNombre = AgregarTextBox(x, y, ancho);
        txtNombre.PlaceholderText = "Ej: Carlos Pérez";
        y += 42;

        AgregarLabel("Teléfono (9 dígitos, empieza con 9)", x, y, ancho, 20);
        y += 22;
        txtTelefono = AgregarTextBox(x, y, ancho);
        txtTelefono.MaxLength = 9;
        txtTelefono.PlaceholderText = "Ej: 987654321";
        txtTelefono.KeyPress += (_, e) =>
        {
            // Solo se permiten dígitos y teclas de control (como Backspace),
            // así se evita escribir letras en un campo que es numérico.
            if (!char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar))
                e.Handled = true;
        };
        txtTelefono.TextChanged += (_, _) => ActualizarTotal();
        y += 42;

        AgregarLabel("Número de personas", x, y, ancho, 20);
        y += 22;
        numPersonas = new NumericUpDown
        {
            Location = new Point(x, y),
            Size = new Size(ancho, 30),
            Minimum = 1,
            Maximum = 100,
            Value = 1,
            BackColor = estilos.ColorBoton,
            ForeColor = estilos.ColorTexto,
            BorderStyle = BorderStyle.FixedSingle
        };
        numPersonas.ValueChanged += (_, _) => ActualizarTotal();
        Controls.Add(numPersonas);
        y += 42;

        AgregarLabel("Tipo de ingreso", x, y, ancho, 20);
        y += 24;

        rbLibre = new RadioButton
        {
            Text = "Libre  (S/ 10.00)",
            Location = new Point(x, y),
            AutoSize = true,
            Checked = true,
            ForeColor = estilos.ColorTexto
        };
        rbHora = new RadioButton
        {
            Text = "Por hora  (S/ 5.00/h)",
            Location = new Point(x + 180, y),
            AutoSize = true,
            ForeColor = estilos.ColorTexto
        };
        rbLibre.CheckedChanged += (_, _) => { ActualizarHoras(); ActualizarTotal(); };
        rbHora.CheckedChanged += (_, _) => { ActualizarHoras(); ActualizarTotal(); };
        Controls.Add(rbLibre);
        Controls.Add(rbHora);
        y += 36;

        lblHoras = AgregarLabel("Número de horas", x, y, ancho, 20);
        lblHoras.Visible = false;
        y += 22;

        numHoras = new NumericUpDown
        {
            Location = new Point(x, y),
            Size = new Size(ancho, 30),
            Minimum = 1,
            Maximum = 24,
            Value = 1,
            BackColor = estilos.ColorBoton,
            ForeColor = estilos.ColorTexto,
            BorderStyle = BorderStyle.FixedSingle,
            Visible = false
        };
        numHoras.ValueChanged += (_, _) => ActualizarTotal();
        Controls.Add(numHoras);
        y += 42;

        lblTotal = new Label
        {
            Text = "Total: —",
            Location = new Point(x, y),
            Size = new Size(ancho, 28),
            Font = new Font("Segoe UI", 12f, FontStyle.Bold),
            ForeColor = estilos.ColorAcento
        };
        Controls.Add(lblTotal);
        y += 44;

        var btnCalcular = CrearBoton("Calcular total", x, y, 182);
        var btnRegistrar = CrearBoton("Registrar", x + 198, y, 182);
        btnRegistrar.BackColor = estilos.ColorAcento;
        btnRegistrar.ForeColor = Color.FromArgb(10, 30, 70);
        btnRegistrar.Font = new Font("Segoe UI", 10f, FontStyle.Bold);

        btnCalcular.Click += BtnCalcular_Click;
        btnRegistrar.Click += BtnRegistrar_Click;
    }

    private void ActualizarHoras()
    {
        lblHoras.Visible = rbHora.Checked;
        numHoras.Visible = rbHora.Checked;
    }

    private void BtnCalcular_Click(object sender, EventArgs e)
    {
        if (!ValidarCampos()) return;
        ActualizarTotal();
    }

    /// <summary>
    /// Calcula y muestra el total a pagar según la cantidad de personas,
    /// el tipo de ingreso y el posible descuento por cliente recurrente.
    /// Se llama tanto desde el botón "Calcular total" como automáticamente
    /// cuando el usuario cambia personas, horas, modalidad o teléfono, para
    /// que el total siempre esté actualizado en pantalla.
    /// </summary>
    private void ActualizarTotal()
    {
        int personas = (int)numPersonas.Value;
        double tarifa = rbLibre.Checked ? 10.0 : (int)numHoras.Value * 5.0;

        string tel = txtTelefono.Text.Trim();
        int visitasPrev = GestorClientes.TelefonoEsValido(tel) ? gestor.ObtenerVisitasPrevias(tel) : 0;
        double descuento = visitasPrev >= 3 ? 5.0 : 0.0;
        double total = personas * tarifa - descuento;

        lblTotal.Text = $"Total: S/ {total:F2}" +
                           (descuento > 0 ? $"  (descuento S/ {descuento:F2})" : "");
    }

    private void BtnRegistrar_Click(object sender, EventArgs e)
    {
        if (!ValidarCampos()) return;
        string resultado = gestor.RegistrarIngresoDesdeUI(
            txtNombre.Text.Trim(), txtTelefono.Text.Trim(),
            (int)numPersonas.Value,
            rbLibre.Checked ? "libre" : "hora",
            rbHora.Checked ? (int)numHoras.Value : 0,
            usuarioActual);
        MessageBox.Show(resultado, "Registro completado",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
        Close();
    }

    /// <summary>
    /// Valida los campos del formulario antes de calcular o registrar.
    /// La validación del teléfono se reutiliza desde
    /// <see cref="GestorClientes.TelefonoEsValido"/> en vez de repetir
    /// aquí la misma lógica (Tema 6: reutilización de funciones).
    /// </summary>
    private bool ValidarCampos()
    {
        if (string.IsNullOrWhiteSpace(txtNombre.Text))
        { MostrarError("Ingresa el nombre del cliente."); return false; }

        string tel = txtTelefono.Text.Trim();
        if (!GestorClientes.TelefonoEsValido(tel))
        { MostrarError("El teléfono debe tener 9 dígitos, solo números, y empezar con 9."); return false; }

        return true;
    }

    private void MostrarError(string msg) =>
        MessageBox.Show(msg, "Error de validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);

    private Label AgregarLabel(string texto, int x, int y, int ancho, int alto,
        Font fuente = null, Color? color = null)
    {
        var lbl = new Label
        {
            Text = texto,
            Location = new Point(x, y),
            Size = new Size(ancho, alto),
            Font = fuente ?? Font,
            ForeColor = color ?? estilos.ColorTextoSuave
        };
        Controls.Add(lbl);
        return lbl;
    }

    private TextBox AgregarTextBox(int x, int y, int ancho)
    {
        var txt = new TextBox
        {
            Location = new Point(x, y),
            Size = new Size(ancho, 28),
            BackColor = estilos.ColorBoton,
            ForeColor = estilos.ColorTexto,
            BorderStyle = BorderStyle.FixedSingle
        };
        Controls.Add(txt);
        return txt;
    }

    private Button CrearBoton(string texto, int x, int y, int ancho)
    {
        var btn = new Button
        {
            Text = texto,
            Location = new Point(x, y),
            Size = new Size(ancho, 40),
            FlatStyle = FlatStyle.Flat,
            BackColor = estilos.ColorBoton,
            ForeColor = estilos.ColorTexto,
            Cursor = Cursors.Hand
        };
        btn.FlatAppearance.BorderSize = 0;
        btn.FlatAppearance.MouseOverBackColor = estilos.ColorBotonHover;
        Controls.Add(btn);
        return btn;
    }
}