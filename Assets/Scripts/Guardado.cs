using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Partida guardada en un archivo JSON de la carpeta de datos del usuario.
/// Se escribe en los puntos de control y al terminar un nivel.
/// </summary>
public static class Guardado
{
    [Serializable]
    public class Partida
    {
        public string nivel = "";            // escena del último punto de control
        public float x, y;                   // posición del punto de control
        public List<string> pergaminos = new List<string>();   // nombres de los pergaminos recogidos
        public List<string> habitantes = new List<string>();   // habitantes ya escuchados
        public List<string> codice = new List<string>();       // entradas desbloqueadas
        public List<string> insignias = new List<string>();
    }

    static Partida datos;
    static string Ruta => Path.Combine(Application.persistentDataPath, "partida.json");

    public static bool Hay => File.Exists(Ruta);
    public static Partida Datos => datos ??= Leer();

    /// Descarta lo que hay en memoria y vuelve a leer el archivo (al elegir "Continuar").
    public static void Recargar() => datos = Leer();

    static Partida Leer()
    {
        try
        {
            if (Hay) return JsonUtility.FromJson<Partida>(File.ReadAllText(Ruta)) ?? new Partida();
        }
        catch (Exception e)
        {
            Debug.LogWarning("No se pudo leer la partida guardada: " + e.Message);   // archivo dañado: se empieza de cero
        }
        return new Partida();
    }

    public static void Guardar()
    {
        try
        {
            // Primero a un archivo temporal: si el juego se cierra a medias, la partida anterior queda intacta
            string temporal = Ruta + ".tmp";
            File.WriteAllText(temporal, JsonUtility.ToJson(Datos, true));
            File.Copy(temporal, Ruta, true);
            File.Delete(temporal);
        }
        catch (Exception e)
        {
            Debug.LogWarning("No se pudo guardar la partida: " + e.Message);
        }
    }

    public static void Borrar()
    {
        datos = new Partida();
        try { if (Hay) File.Delete(Ruta); }
        catch (Exception e) { Debug.LogWarning("No se pudo borrar la partida: " + e.Message); }
    }
}
