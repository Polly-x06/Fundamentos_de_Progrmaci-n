using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace SistemaVittis;

public class ArchivadorClientes
{
    private readonly string rutaArchivo = "clientes.json";

    public void GuardarClientes(Cliente[] lista, int total)
    {
        var listaParaGuardar = new List<Cliente>();
        for (int i = 0; i < total; i++)
            listaParaGuardar.Add(lista[i]);

        try
        {
            var opciones = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(listaParaGuardar, opciones);
            File.WriteAllText(rutaArchivo, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Advertencia: no se pudo guardar clientes.json ({ex.Message}).");
        }
    }

    public int CargarClientes(Cliente[] lista)
    {
        if (!File.Exists(rutaArchivo)) return 0;

        string json = File.ReadAllText(rutaArchivo);
        List<Cliente> clientesCargados;

        try
        {
            clientesCargados = JsonSerializer.Deserialize<List<Cliente>>(json) ?? new List<Cliente>();
        }
        catch (JsonException)
        {
            Console.WriteLine("Advertencia: el archivo clientes.json está corrupto o mal formado, se omite su carga.");
            return 0;
        }

        int total = 0;
        foreach (var cliente in clientesCargados)
        {
            if (total >= lista.Length) break;
            if (cliente == null) continue;

            lista[total] = cliente;
            total++;
        }

        return total;
    }
}
