using System;
using System.IO;

namespace SistemaVittis;

public class RespaldoBinario
{
    private readonly string rutaRespaldo = "clientes.bak";

    public void GuardarRespaldo(Cliente[] lista, int total)
    {
        try
        {
            using BinaryWriter bw = new(File.Open(rutaRespaldo, FileMode.Create));

            bw.Write(total); // primero se guarda cuántos clientes hay

            for (int i = 0; i < total; i++)
            {
                bw.Write(lista[i].Nombre);
                bw.Write(lista[i].Telefono);
                bw.Write(lista[i].HistorialVisitas);
                bw.Write(lista[i].FechaHoraIngreso);
                bw.Write(lista[i].GastoHoy);
                bw.Write(lista[i].RegistradoPor ?? "");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Advertencia: no se pudo guardar clientes.bak ({ex.Message}).");
        }
    }

    public int CargarRespaldo(Cliente[] lista)
    {
        if (!File.Exists(rutaRespaldo)) return 0;

        try
        {
            using BinaryReader br = new(File.Open(rutaRespaldo, FileMode.Open));

            int total = br.ReadInt32();
            int limite = Math.Min(total, lista.Length);

            for (int i = 0; i < limite; i++)
            {
                string nombre = br.ReadString();
                string telefono = br.ReadString();
                int visitas = br.ReadInt32();
                string fecha = br.ReadString();
                double gasto = br.ReadDouble();
                string registradoPor = br.ReadString();
                lista[i] = new Cliente(nombre, telefono, visitas, fecha, gasto, registradoPor);
            }

            return limite;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Advertencia: no se pudo leer clientes.bak ({ex.Message}). Se omite el respaldo.");
            return 0;
        }
    }

    /// <summary>
    /// Recorre el respaldo binario byte por byte usando un puntero, para
    /// demostrar el uso de memoria no administrada (unsafe). El recorrido
    /// en sí no cambia el resultado (el tamaño del archivo es el mismo
    /// que la cantidad de bytes recorridos), pero sirve para comprobar
    /// que se puede leer cada byte del archivo mediante aritmética de
    /// punteros en lugar de un índice de arreglo.
    /// </summary>
    public unsafe long CalcularTamanioConPuntero()
    {
        if (!File.Exists(rutaRespaldo)) return 0;

        byte[] datos = File.ReadAllBytes(rutaRespaldo);
        long bytesRecorridos = 0;

        fixed (byte* p = datos)
        {
            byte* puntero = p;

            for (int i = 0; i < datos.Length; i++)
            {
                bytesRecorridos += *puntero;
                puntero++;
            }
        }

        return datos.Length;
    }
}
