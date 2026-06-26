using System;

namespace SistemaVittis;

public class Cliente
{
    public string Nombre { get; set; }
    public string Telefono { get; set; }
    public int HistorialVisitas { get; set; }
    public string FechaHoraIngreso { get; set; }
    public double GastoHoy { get; set; }
    public string RegistradoPor { get; set; }

    public Cliente(string nombre, string telefono, int historialVisitas,
        string fechaHoraIngreso = null, double gastoHoy = 0.0, string registradoPor = "")
    {
        Nombre = nombre;
        Telefono = telefono;
        HistorialVisitas = historialVisitas;
        FechaHoraIngreso = fechaHoraIngreso ?? DateTime.Now.ToString("dd/MM/yyyy HH:mm");
        GastoHoy = gastoHoy;
        RegistradoPor = registradoPor ?? "";
    }
}