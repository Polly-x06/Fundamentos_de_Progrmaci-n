using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace SistemaVittis;

public class GestorInventario
{
    private readonly string rutaInventario = "productos.json";

    private readonly List<Producto> productos = new();
    private readonly List<MovimientoInventario> movimientos = new();

    public GestorInventario()
    {
        CargarInventario();
    }

    private void CargarInventario()
    {
        productos.Clear();
        movimientos.Clear();
        if (!File.Exists(rutaInventario)) return;

        string json = File.ReadAllText(rutaInventario);
        InventarioData datos;

        try
        {
            datos = JsonSerializer.Deserialize<InventarioData>(json) ?? new InventarioData();
        }
        catch (JsonException)
        {
            Console.WriteLine("Advertencia: el archivo productos.json está corrupto o mal formado, se omite su carga.");
            return;
        }

        foreach (var p in datos.Productos)
            if (p != null) productos.Add(p);

        foreach (var m in datos.Movimientos)
            if (m != null) movimientos.Add(m);
    }

    private void GuardarInventario()
    {
        try
        {
            var datos = new InventarioData { Productos = productos, Movimientos = movimientos };
            var opciones = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(datos, opciones);
            File.WriteAllText(rutaInventario, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Advertencia: no se pudo guardar productos.json ({ex.Message}).");
        }
    }

    public List<Producto> ObtenerProductosPorCategoria(string categoria)
    {
        var resultado = new List<Producto>();
        foreach (var p in productos)
            if (p.Categoria == categoria) resultado.Add(p);
        return resultado;
    }

    private Producto? BuscarProducto(string nombre, string categoria)
    {
        foreach (var p in productos)
            if (p.Categoria == categoria &&
                string.Equals(p.Nombre, nombre, StringComparison.OrdinalIgnoreCase))
                return p;
        return null;
    }

    public string RegistrarEntrada(string nombreProducto, string categoria, int cantidad,
        double precioUnitario, string usuarioActual)
    {
        if (string.IsNullOrWhiteSpace(nombreProducto)) return "Ingresa el nombre del producto.";
        if (cantidad <= 0) return "La cantidad debe ser mayor a 0.";
        if (precioUnitario < 0) return "El precio unitario no puede ser negativo.";

        var producto = BuscarProducto(nombreProducto, categoria);
        if (producto == null)
        {
            producto = new Producto(nombreProducto.Trim(), categoria, cantidad, precioUnitario);
            productos.Add(producto);
        }
        else
        {
            producto.Cantidad += cantidad;
            producto.PrecioUnitario = precioUnitario;
        }

        RegistrarMovimiento("Entrada", producto.Nombre, categoria, cantidad, usuarioActual);
        GuardarInventario();
        return $"Entrada registrada: {cantidad} x {producto.Nombre}.\nStock actual: {producto.Cantidad}";
    }

    public string RegistrarSalida(string nombreProducto, string categoria, int cantidad, string usuarioActual)
    {
        if (string.IsNullOrWhiteSpace(nombreProducto)) return "Ingresa el nombre del producto.";
        if (cantidad <= 0) return "La cantidad debe ser mayor a 0.";

        var producto = BuscarProducto(nombreProducto, categoria);
        if (producto == null) return "Ese producto no existe todavía en esta categoría.";
        if (producto.Cantidad < cantidad) return $"Stock insuficiente. Disponible: {producto.Cantidad}";

        producto.Cantidad -= cantidad;
        RegistrarMovimiento("Salida", producto.Nombre, categoria, cantidad, usuarioActual);
        GuardarInventario();
        return $"Salida registrada: {cantidad} x {producto.Nombre}.\nStock actual: {producto.Cantidad}";
    }

    private void RegistrarMovimiento(string tipo, string producto, string categoria, int cantidad, string usuarioActual)
    {
        string ahora = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
        movimientos.Insert(0, new MovimientoInventario(tipo, producto, categoria, cantidad, ahora, usuarioActual ?? ""));
    }

    public List<MovimientoInventario> ObtenerMovimientos(int max = 100)
    {
        var resultado = new List<MovimientoInventario>();
        for (int i = 0; i < movimientos.Count && i < max; i++)
            resultado.Add(movimientos[i]);
        return resultado;
    }
}
