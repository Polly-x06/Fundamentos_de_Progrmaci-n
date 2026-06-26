using System;
using System.Globalization;

namespace SistemaVittis;

public class GestorClientes
{
    /// <summary>Cantidad máxima de clientes que puede manejar el sistema.</summary>
    private const int CapacidadMaxima = 50;

    private readonly Cliente[] listaClientes = new Cliente[CapacidadMaxima];
    private int totalClientes = 0;
    private readonly ArchivadorClientes archivador = new();
    private readonly RespaldoBinario respaldo = new();

    public void CargarDatosAlIniciar()
    {
        totalClientes = archivador.CargarClientes(listaClientes);

        if (totalClientes == 0)
            totalClientes = respaldo.CargarRespaldo(listaClientes);
    }

    public string RegistrarIngresoDesdeUI(string nombre, string telefono,
        int personas, string modalidad, int horas, string usuarioActual = "")
    {
        nombre = CapitalizarNombre(nombre);

        string ahora = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
        string hoy = DateTime.Now.ToString("dd/MM/yyyy");

        double tarifa = modalidad == "libre" ? 10.0 : horas * 5.0;
        int indice = BuscarIndicePorTelefono(telefono);
        int visitasPrev = indice != -1 ? listaClientes[indice].HistorialVisitas : 0;
        double descuento = visitasPrev >= 3 ? 5.0 : 0.0;
        double totalFinal = personas * tarifa - descuento;

        string mensaje;

        if (indice != -1)
        {
            var cliente = listaClientes[indice];
            cliente.HistorialVisitas++;
            cliente.RegistradoPor = usuarioActual ?? "";

            bool mismoDia = cliente.FechaHoraIngreso?.StartsWith(hoy) == true;
            cliente.GastoHoy = mismoDia ? cliente.GastoHoy + totalFinal : totalFinal;
            cliente.FechaHoraIngreso = ahora;

            mensaje = $"Cliente recurrente registrado.\n" +
                      $"Visitas acumuladas: {cliente.HistorialVisitas}\n" +
                      $"Total a pagar: {Utilidades.FormatearMoneda(totalFinal)}" +
                      (descuento > 0 ? $"\nDescuento aplicado: {Utilidades.FormatearMoneda(descuento)}" : "") +
                      $"\nGastado hoy en total: {Utilidades.FormatearMoneda(cliente.GastoHoy)}";
        }
        else
        {
            if (totalClientes >= CapacidadMaxima)
                return $"Se ha alcanzado el límite máximo de {CapacidadMaxima} clientes.";

            listaClientes[totalClientes] = new Cliente(nombre, telefono, 1, ahora, totalFinal, usuarioActual ?? "");
            totalClientes++;
            mensaje = $"Nuevo cliente registrado.\n" +
                      $"Total a pagar: {Utilidades.FormatearMoneda(totalFinal)}";
        }

        GuardarEnAmbosFormatos();
        return mensaje;
    }

    public bool EliminarCliente(int indice)
    {
        if (indice < 0 || indice >= totalClientes) return false;

        for (int i = indice; i < totalClientes - 1; i++)
            listaClientes[i] = listaClientes[i + 1];

        listaClientes[totalClientes - 1] = null!;
        totalClientes--;

        GuardarEnAmbosFormatos();
        return true;
    }

    public void EliminarTodo()
    {
        for (int i = 0; i < totalClientes; i++)
            listaClientes[i] = null!;

        totalClientes = 0;
        GuardarEnAmbosFormatos();
    }

    public Cliente[] BuscarClientes(string textoBusqueda)
    {
        Cliente[] todos = ObtenerClientes();

        if (string.IsNullOrWhiteSpace(textoBusqueda))
            return todos;

        string filtro = textoBusqueda.Trim().ToLower();
        int total = 0;
        Cliente[] resultado = new Cliente[todos.Length];

        foreach (var c in todos)
        {
            bool coincideNombre = c.Nombre.ToLower().Contains(filtro);
            bool coincideTelefono = c.Telefono.Contains(filtro);

            if (coincideNombre || coincideTelefono)
            {
                resultado[total] = c;
                total++;
            }
        }

        Cliente[] final = new Cliente[total];
        Array.Copy(resultado, final, total);
        return final;
    }

    public Cliente[] ObtenerClientes()
    {
        Cliente[] resultado = new Cliente[totalClientes];
        Array.Copy(listaClientes, resultado, totalClientes);
        return resultado;
    }

    public Cliente[] ObtenerClientesDeHoy()
    {
        string hoy = DateTime.Now.ToString("dd/MM/yyyy");
        Cliente[] todos = ObtenerClientes();
        int total = 0;
        Cliente[] resultado = new Cliente[todos.Length];

        foreach (var c in todos)
        {
            if (c.FechaHoraIngreso != null && c.FechaHoraIngreso.StartsWith(hoy))
            {
                resultado[total] = c;
                total++;
            }
        }

        Cliente[] final = new Cliente[total];
        Array.Copy(resultado, final, total);
        return final;
    }

    public bool AgregarGastoAClientePorTelefono(string telefono, double monto)
    {
        int indice = BuscarIndicePorTelefono(telefono);
        if (indice == -1) return false;

        listaClientes[indice].GastoHoy += monto;
        GuardarEnAmbosFormatos();
        return true;
    }

    public int ObtenerVisitasPrevias(string telefono)
    {
        int indice = BuscarIndicePorTelefono(telefono);
        return indice != -1 ? listaClientes[indice].HistorialVisitas : 0;
    }

    public void OrdenarClientesPorNombre()
    {
        if (totalClientes < 2) return;

        for (int i = 0; i < totalClientes - 1; i++)
            for (int j = 0; j < totalClientes - i - 1; j++)
                if (string.Compare(listaClientes[j].Nombre, listaClientes[j + 1].Nombre,
                        StringComparison.CurrentCultureIgnoreCase) > 0)
                    (listaClientes[j], listaClientes[j + 1]) = (listaClientes[j + 1], listaClientes[j]);

        GuardarEnAmbosFormatos();
    }

    public long ObtenerTamanioRespaldoBinario() => respaldo.CalcularTamanioConPuntero();

    public static bool TelefonoEsValido(string telefono) =>
        !string.IsNullOrEmpty(telefono) &&
        telefono.Length == 9 && telefono[0] == '9' && Utilidades.EsSoloNumeros(telefono);

    private void GuardarEnAmbosFormatos()
    {
        archivador.GuardarClientes(listaClientes, totalClientes);
        respaldo.GuardarRespaldo(listaClientes, totalClientes);
    }

    public int BuscarIndicePorTelefonoPublico(string telefono) => BuscarIndicePorTelefono(telefono);

    private int BuscarIndicePorTelefono(string telefono)
    {
        for (int i = 0; i < totalClientes; i++)
            if (listaClientes[i].Telefono == telefono) return i;
        return -1;
    }

    /// <summary>
    /// Pone en mayúscula la primera letra de cada palabra del nombre
    /// (ej. "carlos perez" -> "Carlos Perez"). Se ignoran los espacios
    /// repetidos que el usuario pueda escribir por error, para que el
    /// resultado no quede con espacios dobles.
    /// </summary>
    private static string CapitalizarNombre(string nombre)
    {
        string[] palabras = nombre.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < palabras.Length; i++)
            palabras[i] = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(palabras[i]);
        return string.Join(" ", palabras);
    }
}
