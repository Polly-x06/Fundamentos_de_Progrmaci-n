using System;
using System.Drawing;
using System.Windows.Forms;

namespace SistemaVittis;

public class FormInventario : Form
{
    private readonly GestorInventario gestor;
    private readonly GestorClientes gestorClientes;
    private readonly EstilosVittis estilos;
    private readonly string usuarioActual;

    private ComboBox cboCategoria;
    private DataGridView gridProductos;
    private DataGridView gridMovimientos;

    private ComboBox cboProducto;
    private NumericUpDown numCantidad;
    private NumericUpDown numPrecio;
    private Label lblPrecio;
    private ComboBox cboCliente;

    public FormInventario(GestorInventario gestor, EstilosVittis estilos, string usuarioActual = "",
        GestorClientes gestorClientes = null)
    {
        this.gestor = gestor;
        this.gestorClientes = gestorClientes;
        this.estilos = estilos;
        this.usuarioActual = usuarioActual ?? "";
        InicializarComponentes();
        CargarProductos();
        CargarMovimientos();
        CargarClientesDeHoy();
    }

    private void InicializarComponentes()
    {
        Text = "Inventario — Vittis";
        Size = new Size(900, 800);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = estilos.ColorFondo;
        ForeColor = estilos.ColorTexto;
        Font = new Font("Segoe UI", 10f);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;

        int x = 20;

        Controls.Add(new Label
        {
            Text = "Entradas y salidas de inventario",
            Font = new Font("Segoe UI", 14f, FontStyle.Bold),
            ForeColor = estilos.ColorAcento,
            Location = new Point(x, 18),
            AutoSize = true
        });

        Controls.Add(new Label
        {
            Text = "Categoría:",
            Location = new Point(x, 58),
            AutoSize = true,
            ForeColor = estilos.ColorTextoSuave
        });

        cboCategoria = new ComboBox
        {
            Location = new Point(x + 80, 54),
            Size = new Size(230, 28),
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = estilos.ColorBoton,
            ForeColor = estilos.ColorTexto,
            FlatStyle = FlatStyle.Flat
        };
        cboCategoria.Items.AddRange(CategoriasInventario.Todas);
        cboCategoria.SelectedIndex = 0;
        cboCategoria.SelectedIndexChanged += (_, _) => CargarProductos();
        Controls.Add(cboCategoria);

        // --- Grid de productos de la categoría seleccionada ---
        gridProductos = new DataGridView
        {
            Location = new Point(x, 96),
            Size = new Size(840, 200),
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
        EstilizarGrid(gridProductos);
        gridProductos.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Producto", FillWeight = 36 });
        gridProductos.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Cantidad", FillWeight = 18 });
        gridProductos.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Precio unitario", FillWeight = 23 });
        gridProductos.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Valor en stock", FillWeight = 23 });
        gridProductos.CellClick += GridProductos_CellClick;
        Controls.Add(gridProductos);

        // --- Panel de movimiento (entrada / salida) ---
        int y2 = 310;
        Controls.Add(new Label
        {
            Text = "Registrar movimiento",
            Font = new Font("Segoe UI", 11.5f, FontStyle.Bold),
            ForeColor = estilos.ColorAcento,
            Location = new Point(x, y2),
            AutoSize = true
        });
        y2 += 32;

        Controls.Add(new Label { Text = "Producto", Location = new Point(x, y2), AutoSize = true, ForeColor = estilos.ColorTextoSuave });
        cboProducto = new ComboBox
        {
            Location = new Point(x, y2 + 20),
            Size = new Size(280, 28),
            DropDownStyle = ComboBoxStyle.DropDown, // desplegable + editable (permite escribir un producto nuevo)
            AutoCompleteMode = AutoCompleteMode.SuggestAppend,
            AutoCompleteSource = AutoCompleteSource.ListItems,
            BackColor = estilos.ColorBoton,
            ForeColor = estilos.ColorTexto,
            FlatStyle = FlatStyle.Flat
        };
        cboProducto.SelectedIndexChanged += CboProducto_SelectedIndexChanged;
        Controls.Add(cboProducto);

        Controls.Add(new Label { Text = "Cantidad", Location = new Point(x + 300, y2), AutoSize = true, ForeColor = estilos.ColorTextoSuave });
        numCantidad = new NumericUpDown
        {
            Location = new Point(x + 300, y2 + 20),
            Size = new Size(110, 28),
            Minimum = 1,
            Maximum = 100000,
            Value = 1,
            BackColor = estilos.ColorBoton,
            ForeColor = estilos.ColorTexto,
            BorderStyle = BorderStyle.FixedSingle
        };
        Controls.Add(numCantidad);

        lblPrecio = new Label { Text = "Precio unitario (S/)", Location = new Point(x + 430, y2), AutoSize = true, ForeColor = estilos.ColorTextoSuave };
        Controls.Add(lblPrecio);
        numPrecio = new NumericUpDown
        {
            Location = new Point(x + 430, y2 + 20),
            Size = new Size(130, 28),
            Minimum = 0,
            Maximum = 10000,
            DecimalPlaces = 2,
            Increment = 0.50M,
            Value = 1,
            BackColor = estilos.ColorBoton,
            ForeColor = estilos.ColorTexto,
            BorderStyle = BorderStyle.FixedSingle
        };
        Controls.Add(numPrecio);

        int y2b = y2 + 58;
        Controls.Add(new Label
        {
            Text = "Cliente (cargo a \"Gastó hoy\" en Salida — opcional)",
            Location = new Point(x, y2b),
            AutoSize = true,
            ForeColor = estilos.ColorTextoSuave
        });
        cboCliente = new ComboBox
        {
            Location = new Point(x, y2b + 20),
            Size = new Size(450, 28),
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = estilos.ColorBoton,
            ForeColor = estilos.ColorTexto,
            FlatStyle = FlatStyle.Flat
        };
        Controls.Add(cboCliente);

        var btnRefrescarClientes = new Button
        {
            Text = "🔄",
            Location = new Point(x + 460, y2b + 20),
            Size = new Size(40, 28),
            FlatStyle = FlatStyle.Flat,
            BackColor = estilos.ColorBoton,
            ForeColor = estilos.ColorTexto,
            Cursor = Cursors.Hand
        };
        btnRefrescarClientes.FlatAppearance.BorderSize = 0;
        btnRefrescarClientes.FlatAppearance.MouseOverBackColor = estilos.ColorBotonHover;
        btnRefrescarClientes.Click += (_, _) => CargarClientesDeHoy();
        Controls.Add(btnRefrescarClientes);

        y2 = y2b + 58;
        var btnEntrada = new Button
        {
            Text = "⬇  Registrar Entrada",
            Location = new Point(x, y2),
            Size = new Size(200, 40),
            FlatStyle = FlatStyle.Flat,
            BackColor = estilos.ColorAcento,
            ForeColor = Color.FromArgb(10, 30, 70),
            Font = new Font("Segoe UI", 10f, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        btnEntrada.FlatAppearance.BorderSize = 0;
        btnEntrada.Click += (_, _) => RegistrarEntrada();
        Controls.Add(btnEntrada);

        var btnSalida = new Button
        {
            Text = "⬆  Registrar Salida",
            Location = new Point(x + 220, y2),
            Size = new Size(200, 40),
            FlatStyle = FlatStyle.Flat,
            BackColor = estilos.ColorBoton,
            ForeColor = estilos.ColorTexto,
            Font = new Font("Segoe UI", 10f, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        btnSalida.FlatAppearance.BorderSize = 0;
        btnSalida.FlatAppearance.MouseOverBackColor = estilos.ColorBotonHover;
        btnSalida.Click += (_, _) => RegistrarSalida();
        Controls.Add(btnSalida);

        // --- Historial de movimientos ---
        int y3 = y2 + 60;
        Controls.Add(new Label
        {
            Text = "Historial de movimientos (entradas y salidas)",
            Font = new Font("Segoe UI", 11.5f, FontStyle.Bold),
            ForeColor = estilos.ColorAcento,
            Location = new Point(x, y3),
            AutoSize = true
        });
        y3 += 32;

        gridMovimientos = new DataGridView
        {
            Location = new Point(x, y3),
            Size = new Size(840, 200),
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
            Font = new Font("Segoe UI", 9.5f)
        };
        EstilizarGrid(gridMovimientos);
        gridMovimientos.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Tipo", FillWeight = 12 });
        gridMovimientos.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Producto", FillWeight = 24 });
        gridMovimientos.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Categoría", FillWeight = 20 });
        gridMovimientos.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Cantidad", FillWeight = 12 });
        gridMovimientos.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Fecha y hora", FillWeight = 18 });
        gridMovimientos.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Registrado por", FillWeight = 18 });
        Controls.Add(gridMovimientos);
    }

    private void EstilizarGrid(DataGridView g)
    {
        g.ColumnHeadersDefaultCellStyle.BackColor = estilos.ColorAcento;
        g.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(10, 30, 70);
        g.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
        g.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        g.ColumnHeadersHeight = 34;
        g.DefaultCellStyle.BackColor = estilos.ColorBoton;
        g.DefaultCellStyle.ForeColor = estilos.ColorTexto;
        g.DefaultCellStyle.SelectionBackColor = estilos.ColorAcentoAlt;
        g.DefaultCellStyle.SelectionForeColor = Color.White;
        g.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(20, 55, 110);
    }

    private string CategoriaSeleccionada => cboCategoria.SelectedItem?.ToString() ?? CategoriasInventario.Snacks;

    private void CargarProductos()
    {
        gridProductos.Rows.Clear();

        string textoActual = cboProducto?.Text ?? "";
        cboProducto?.Items.Clear();

        foreach (var p in gestor.ObtenerProductosPorCategoria(CategoriaSeleccionada))
        {
            gridProductos.Rows.Add(p.Nombre, p.Cantidad,
                Utilidades.FormatearMoneda(p.PrecioUnitario),
                Utilidades.FormatearMoneda(p.ValorStock));

            cboProducto?.Items.Add(p.Nombre);
        }

        if (cboProducto != null) cboProducto.Text = textoActual;
    }

    private void GridProductos_CellClick(object sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0) return;
        cboProducto.Text = gridProductos.Rows[e.RowIndex].Cells[0].Value?.ToString() ?? "";
    }

    private void CboProducto_SelectedIndexChanged(object sender, EventArgs e)
    {
        // Si el producto elegido ya existe, se sugiere su precio actual
        // (útil para Entrada de reabastecimiento; en Salida no afecta nada).
        foreach (var p in gestor.ObtenerProductosPorCategoria(CategoriaSeleccionada))
        {
            if (string.Equals(p.Nombre, cboProducto.Text, StringComparison.OrdinalIgnoreCase))
            {
                numPrecio.Value = (decimal)p.PrecioUnitario;
                break;
            }
        }
    }

    private void CargarClientesDeHoy()
    {
        if (cboCliente == null) return;

        cboCliente.Items.Clear();
        cboCliente.Items.Add("— Ninguno —");

        if (gestorClientes != null)
        {
            foreach (var c in gestorClientes.ObtenerClientesDeHoy())
                cboCliente.Items.Add($"{c.Nombre} ({c.Telefono})");
        }

        cboCliente.SelectedIndex = 0;
    }

    private string ObtenerTelefonoClienteSeleccionado()
    {
        if (cboCliente == null || cboCliente.SelectedIndex <= 0) return null;

        string texto = cboCliente.SelectedItem?.ToString() ?? "";
        int inicio = texto.LastIndexOf('(');
        int fin = texto.LastIndexOf(')');
        if (inicio == -1 || fin == -1 || fin <= inicio) return null;

        return texto.Substring(inicio + 1, fin - inicio - 1);
    }

    private void CargarMovimientos()
    {
        gridMovimientos.Rows.Clear();
        foreach (var m in gestor.ObtenerMovimientos())
        {
            gridMovimientos.Rows.Add(m.Tipo, m.Producto, m.Categoria, m.Cantidad, m.FechaHora,
                string.IsNullOrWhiteSpace(m.RegistradoPor) ? "—" : m.RegistradoPor);
        }
    }

    /// <summary>
    /// ENTRADA = ingreso de stock al inventario (ej. compra a proveedor).
    /// No tiene relación con clientes ni afecta "Gastó hoy".
    /// </summary>
    private void RegistrarEntrada()
    {
        string resultado = gestor.RegistrarEntrada(cboProducto.Text.Trim(), CategoriaSeleccionada,
            (int)numCantidad.Value, (double)numPrecio.Value, usuarioActual);

        MessageBox.Show(resultado, "Entrada de inventario", MessageBoxButtons.OK, MessageBoxIcon.Information);
        CargarProductos();
        CargarMovimientos();
    }

    /// <summary>
    /// SALIDA = venta/consumo de un producto del inventario. Si se eligió un
    /// cliente de hoy, el monto (cantidad x precio unitario) se suma a su
    /// "Gastó hoy" en clientes.json.
    /// </summary>
    private void RegistrarSalida()
    {
        string nombreProducto = cboProducto.Text.Trim();
        int cantidad = (int)numCantidad.Value;
        string telefonoCliente = ObtenerTelefonoClienteSeleccionado();

        // Se busca el precio del producto ANTES de la salida, para poder
        // calcular el cargo que se aplicará al cliente seleccionado.
        double precioUnitario = 0.0;
        foreach (var p in gestor.ObtenerProductosPorCategoria(CategoriaSeleccionada))
        {
            if (string.Equals(p.Nombre, nombreProducto, StringComparison.OrdinalIgnoreCase))
            {
                precioUnitario = p.PrecioUnitario;
                break;
            }
        }

        string resultado = gestor.RegistrarSalida(nombreProducto, CategoriaSeleccionada, cantidad, usuarioActual);

        if (!string.IsNullOrEmpty(telefonoCliente) && resultado.StartsWith("Salida registrada"))
        {
            double monto = cantidad * precioUnitario;
            bool cargado = gestorClientes != null && gestorClientes.AgregarGastoAClientePorTelefono(telefonoCliente, monto);

            resultado += cargado
                ? $"\n\nSe agregó {Utilidades.FormatearMoneda(monto)} a \"Gastó hoy\" del cliente seleccionado."
                : "\n\nNo se pudo aplicar el cargo al cliente seleccionado.";
        }

        MessageBox.Show(resultado, "Salida de inventario", MessageBoxButtons.OK, MessageBoxIcon.Information);
        CargarProductos();
        CargarMovimientos();
        CargarClientesDeHoy();
    }
}
