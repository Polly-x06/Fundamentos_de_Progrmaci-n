using System;
using System.Drawing;
using System.Windows.Forms;

namespace SistemaVittis;

public class FormReporte : Form
{
    private readonly GestorClientes gestor;
    private readonly EstilosVittis estilos;
    private readonly string hoy = DateTime.Now.ToString("dd/MM/yyyy");

    private DataGridView grid;
    private TextBox txtBuscar;
    private Label lblTotal;
    private Label lblTotalGastado;

    public FormReporte(GestorClientes gestor, EstilosVittis estilos)
    {
        this.gestor = gestor;
        this.estilos = estilos;
        InicializarComponentes();
        CargarDatos();
    }

    private void InicializarComponentes()
    {
        Text = "Reporte Diario — Vittis";
        Size = new Size(720, 560);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = estilos.ColorFondo;
        ForeColor = estilos.ColorTexto;
        Font = new Font("Segoe UI", 10f);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;

        Controls.Add(new Label
        {
            Text = $"Reporte del día  {hoy}",
            Font = new Font("Segoe UI", 14f, FontStyle.Bold),
            ForeColor = estilos.ColorAcento,
            Location = new Point(20, 20),
            AutoSize = true
        });

        // --- Buscador por nombre o teléfono, dentro de los clientes del día ---
        Controls.Add(new Label
        {
            Text = "Buscar por nombre o teléfono:",
            Font = new Font("Segoe UI", 9f),
            ForeColor = estilos.ColorTextoSuave,
            Location = new Point(20, 56),
            AutoSize = true
        });

        txtBuscar = new TextBox
        {
            Location = new Point(20, 76),
            Size = new Size(480, 28),
            BackColor = estilos.ColorBoton,
            ForeColor = estilos.ColorTexto,
            BorderStyle = BorderStyle.FixedSingle,
            PlaceholderText = "Ej: Carlos  o  987654321"
        };
        txtBuscar.TextChanged += (_, _) => CargarDatos();
        Controls.Add(txtBuscar);

        var btnLimpiar = new Button
        {
            Text = "Limpiar",
            Location = new Point(510, 76),
            Size = new Size(90, 28),
            FlatStyle = FlatStyle.Flat,
            BackColor = estilos.ColorBoton,
            ForeColor = estilos.ColorTexto,
            Cursor = Cursors.Hand
        };
        btnLimpiar.FlatAppearance.BorderSize = 0;
        btnLimpiar.FlatAppearance.MouseOverBackColor = estilos.ColorBotonHover;
        btnLimpiar.Click += (_, _) => txtBuscar.Clear();
        Controls.Add(btnLimpiar);

        grid = new DataGridView
        {
            Location = new Point(20, 116),
            Size = new Size(670, 330),
            BackgroundColor = estilos.ColorFondo,
            ForeColor = estilos.ColorTexto,
            GridColor = Color.FromArgb(30, 60, 120),
            BorderStyle = BorderStyle.None,
            RowHeadersVisible = false,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            Font = new Font("Segoe UI", 10f)
        };

        grid.ColumnHeadersDefaultCellStyle.BackColor = estilos.ColorAcento;
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(10, 30, 70);
        grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
        grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        grid.ColumnHeadersHeight = 38;
        grid.DefaultCellStyle.BackColor = estilos.ColorBoton;
        grid.DefaultCellStyle.ForeColor = estilos.ColorTexto;
        grid.DefaultCellStyle.SelectionBackColor = estilos.ColorAcentoAlt;
        grid.DefaultCellStyle.SelectionForeColor = Color.White;
        grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(20, 55, 110);

        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Nombre", FillWeight = 26 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Teléfono", FillWeight = 18 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Hora de ingreso", FillWeight = 18 });
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Gastado hoy",
            FillWeight = 16,
            DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight }
        });
        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Registrado por", FillWeight = 22 });
        Controls.Add(grid);

        lblTotal = new Label
        {
            Text = "",
            Font = new Font("Segoe UI", 11f, FontStyle.Bold),
            ForeColor = estilos.ColorAcento,
            Location = new Point(20, 460),
            AutoSize = true
        };
        Controls.Add(lblTotal);

        lblTotalGastado = new Label
        {
            Text = "",
            Font = new Font("Segoe UI", 11f, FontStyle.Bold),
            ForeColor = estilos.ColorAcento,
            Location = new Point(360, 460),
            AutoSize = true
        };
        Controls.Add(lblTotalGastado);

        // Subtema 5.4: aplicación básica del respaldo binario persistente.
        long bytesRespaldo = gestor.ObtenerTamanioRespaldoBinario();
        Controls.Add(new Label
        {
            Text = $"Respaldo binario (clientes.bak): {bytesRespaldo} bytes",
            Font = new Font("Segoe UI", 9f),
            ForeColor = estilos.ColorTextoSuave,
            Location = new Point(20, 490),
            AutoSize = true
        });
    }

    /// <summary>
    /// Recarga el grid con los clientes del día actual, aplicando además
    /// el filtro de búsqueda por nombre o teléfono. Reutiliza
    /// <see cref="GestorClientes.BuscarClientes"/>, la misma función que
    /// usa el Historial (Tema 6: reutilización de funciones).
    /// </summary>
    private void CargarDatos()
    {
        grid.Rows.Clear();
        int contador = 0;
        double totalGastado = 0;

        foreach (var c in gestor.BuscarClientes(txtBuscar.Text))
        {
            if (c.FechaHoraIngreso?.StartsWith(hoy) == true)
            {
                grid.Rows.Add(c.Nombre, c.Telefono, c.FechaHoraIngreso, Utilidades.FormatearMoneda(c.GastoHoy),
                    string.IsNullOrWhiteSpace(c.RegistradoPor) ? "—" : c.RegistradoPor);
                contador++;
                totalGastado += c.GastoHoy;
            }
        }

        bool hayFiltro = !string.IsNullOrWhiteSpace(txtBuscar.Text);

        lblTotal.Text = contador == 0
            ? (hayFiltro ? "No se encontraron coincidencias hoy." : "No hay clientes registrados hoy.")
            : (hayFiltro
                ? $"Coincidencias encontradas hoy: {contador}"
                : $"Total de clientes atendidos hoy: {contador}");
        lblTotal.ForeColor = contador == 0 ? estilos.ColorTextoSuave : estilos.ColorAcento;

        lblTotalGastado.Text = contador == 0
            ? ""
            : $"Total gastado hoy: {Utilidades.FormatearMoneda(totalGastado)}";
    }
}
