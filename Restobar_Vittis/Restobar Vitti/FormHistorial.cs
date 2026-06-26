using System;
using System.Drawing;
using System.Windows.Forms;

namespace SistemaVittis;

public class FormHistorial : Form
{
    private readonly GestorClientes gestor;
    private readonly EstilosVittis estilos;
    private DataGridView grid;
    private TextBox txtBuscar;
    private Label lblResultados;

    public FormHistorial(GestorClientes gestor, EstilosVittis estilos)
    {
        this.gestor = gestor;
        this.estilos = estilos;
        InicializarComponentes();
        CargarDatos();
    }

    private void InicializarComponentes()
    {
        Text = "Historial de Clientes — Vittis";
        Size = new Size(740, 600);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = estilos.ColorFondo;
        ForeColor = estilos.ColorTexto;
        Font = new Font("Segoe UI", 10f);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;

        Controls.Add(new Label
        {
            Text = "Historial de Clientes",
            Font = new Font("Segoe UI", 14f, FontStyle.Bold),
            ForeColor = estilos.ColorAcento,
            Location = new Point(20, 20),
            AutoSize = true
        });

        // --- Buscador por nombre o teléfono ---
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
            Size = new Size(500, 28),
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
            Location = new Point(530, 76),
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

        lblResultados = new Label
        {
            Text = "",
            Font = new Font("Segoe UI", 8.5f),
            ForeColor = estilos.ColorTextoSuave,
            Location = new Point(20, 108),
            AutoSize = true
        };
        Controls.Add(lblResultados);

        // --- Grid de resultados ---
        grid = new DataGridView
        {
            Location = new Point(20, 132),
            Size = new Size(690, 380),
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
        AplicarEstiloGrid(grid);

        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Nombre", FillWeight = 24 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Teléfono", FillWeight = 16 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Visitas", FillWeight = 10 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Último ingreso", FillWeight = 18 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Registrado por", FillWeight = 18 });

        var colEliminar = new DataGridViewButtonColumn
        {
            HeaderText = "Acción",
            Text = "🗑 Eliminar",
            UseColumnTextForButtonValue = true,
            FillWeight = 10,
            FlatStyle = FlatStyle.Flat,
            DefaultCellStyle =
            {
                BackColor = estilos.ColorPeligro,
                ForeColor = Color.White,
                Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleCenter
            }
        };
        grid.Columns.Add(colEliminar);

        grid.CellClick += Grid_CellClick;
        Controls.Add(grid);
    }

    /// <summary>
    /// Recarga el grid aplicando el filtro de búsqueda actual. Reutiliza
    /// <see cref="GestorClientes.BuscarClientes"/> para no duplicar la
    /// lógica de filtrado (Tema 6: reutilización de funciones).
    /// </summary>
    private void CargarDatos()
    {
        grid.Rows.Clear();
        Cliente[] resultados = gestor.BuscarClientes(txtBuscar.Text);

        foreach (var c in resultados)
            grid.Rows.Add(c.Nombre, c.Telefono, c.HistorialVisitas, c.FechaHoraIngreso,
                string.IsNullOrWhiteSpace(c.RegistradoPor) ? "—" : c.RegistradoPor);

        lblResultados.Text = string.IsNullOrWhiteSpace(txtBuscar.Text)
            ? $"Mostrando todos los clientes ({resultados.Length})"
            : $"Se encontraron {resultados.Length} resultado(s) para \"{txtBuscar.Text.Trim()}\"";
    }

    /// <summary>
    /// Al eliminar, se usa el TELÉFONO de la fila (no el índice de fila)
    /// para ubicar al cliente real en el gestor. Esto es necesario porque,
    /// al estar filtrando con el buscador, la posición visible en el grid
    /// puede no coincidir con la posición real en la lista completa.
    /// </summary>
    private void Grid_CellClick(object sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex != grid.Columns.Count - 1) return;

        string nombre = grid.Rows[e.RowIndex].Cells[0].Value?.ToString() ?? "";
        string telefono = grid.Rows[e.RowIndex].Cells[1].Value?.ToString() ?? "";

        var confirmacion = MessageBox.Show(
            $"¿Deseas eliminar el registro de \"{nombre}\"?\nEsta acción no se puede deshacer.",
            "Confirmar eliminación",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (confirmacion != DialogResult.Yes) return;

        int indiceReal = gestor.BuscarIndicePorTelefonoPublico(telefono);

        if (indiceReal != -1 && gestor.EliminarCliente(indiceReal))
        {
            MessageBox.Show("Registro eliminado correctamente.", "Listo",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        else
        {
            MessageBox.Show("No se pudo eliminar el registro. Intenta de nuevo.", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        // Siempre recargamos el grid para que refleje el estado real,
        // independientemente del resultado de la eliminación.
        CargarDatos();
    }

    private void AplicarEstiloGrid(DataGridView g)
    {
        g.ColumnHeadersDefaultCellStyle.BackColor = estilos.ColorAcento;
        g.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(10, 30, 70);
        g.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
        g.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        g.ColumnHeadersHeight = 38;
        g.DefaultCellStyle.BackColor = estilos.ColorBoton;
        g.DefaultCellStyle.ForeColor = estilos.ColorTexto;
        g.DefaultCellStyle.SelectionBackColor = estilos.ColorAcentoAlt;
        g.DefaultCellStyle.SelectionForeColor = Color.White;
        g.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(20, 55, 110);
    }
}
