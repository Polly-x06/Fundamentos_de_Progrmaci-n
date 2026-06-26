using System;
using System.Collections.Generic;

namespace SistemaVittis;

public static class CategoriasInventario
{
    public const string Snacks = "Snacks";
    public const string Bebidas = "Bebidas";
    public const string BebidasAlcoholicas = "Bebidas alcohólicas";
    public const string ArticulosPiscina = "Artículos de piscina";

    public static readonly string[] Todas =
    {
        Snacks, Bebidas, BebidasAlcoholicas, ArticulosPiscina
    };
}

public class Producto
{
    public string Nombre { get; set; }
    public string Categoria { get; set; }
    public int Cantidad { get; set; }
    public double PrecioUnitario { get; set; }

    public double ValorStock => Cantidad * PrecioUnitario;

    public Producto(string nombre, string categoria, int cantidad, double precioUnitario)
    {
        Nombre = nombre;
        Categoria = categoria;
        Cantidad = cantidad;
        PrecioUnitario = precioUnitario;
    }
}
public class MovimientoInventario
{
    public string Tipo { get; set; }           // "Entrada" o "Salida"
    public string Producto { get; set; }
    public string Categoria { get; set; }
    public int Cantidad { get; set; }
    public string FechaHora { get; set; }
    public string RegistradoPor { get; set; }

    public MovimientoInventario(string tipo, string producto, string categoria,
        int cantidad, string fechaHora, string registradoPor)
    {
        Tipo = tipo;
        Producto = producto;
        Categoria = categoria;
        Cantidad = cantidad;
        FechaHora = fechaHora;
        RegistradoPor = registradoPor ?? "";
    }
}

/// <summary>
/// Contenedor que agrupa productos y movimientos en un único archivo
/// productos.json, en lugar de mantenerlos en archivos separados.
/// </summary>
public class InventarioData
{
    public List<Producto> Productos { get; set; } = new();
    public List<MovimientoInventario> Movimientos { get; set; } = new();
}
