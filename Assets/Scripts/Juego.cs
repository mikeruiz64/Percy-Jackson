using UnityEngine;
using UnityEngine.SceneManagement;

/// En cada momento el juego está en un solo estado. Solo en Jugando se mueve Alexios y corre el tiempo.
public enum Estado { Menu, Jugando, Dialogo, Codice, Pausa, Recompensa }

/// <summary>
/// Estado general del juego y cambio entre escenas. Se crea solo al arrancar y vive toda la partida.
/// </summary>
public class Juego : MonoBehaviour
{
    public static Estado Estado { get; private set; } = Estado.Jugando;
    public static event System.Action<Estado> AlCambiar;

    static bool empezarJugando;      // la siguiente escena arranca sin pasar por el menú
    static int fotogramaCambio = -1;

    /// Cierto en el fotograma en que cambia el estado: la tecla que cerró un menú no debe valer también como salto.
    public static bool AcabaDeCambiar => Time.frameCount == fotogramaCambio;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Crear()
    {
        var objeto = new GameObject("Juego");
        DontDestroyOnLoad(objeto);
        objeto.AddComponent<Juego>();
        objeto.AddComponent<Interfaz>();
        SceneManager.sceneLoaded += (escena, modo) => AlCargarEscena();
    }

    static void AlCargarEscena()
    {
        // Las escenas de prueba sin Alexios no tienen menú
        bool hayAlexios = FindFirstObjectByType<Alexios>() != null;
        Cambiar(hayAlexios && !empezarJugando ? Estado.Menu : Estado.Jugando);
        empezarJugando = false;
    }

    public static void Cambiar(Estado nuevo)
    {
        Estado = nuevo;
        fotogramaCambio = Time.frameCount;
        Time.timeScale = nuevo == Estado.Jugando ? 1f : 0f;
        AlCambiar?.Invoke(nuevo);
    }

    public static void NuevaPartida()
    {
        Guardado.Borrar();
        empezarJugando = true;
        SceneManager.LoadScene(0);
    }

    public static void Continuar()
    {
        Guardado.Recargar();
        empezarJugando = true;
        // Si el nivel guardado ya no existe en esta versión, se empieza por el primero
        if (Application.CanStreamedLevelBeLoaded(Guardado.Datos.nivel)) SceneManager.LoadScene(Guardado.Datos.nivel);
        else SceneManager.LoadScene(0);
    }

    public static void VolverAlMenu() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

    public static void Salir() => Application.Quit();

    void Update()
    {
        if (!Controles.Pausa) return;
        if (Estado == Estado.Jugando) Cambiar(Estado.Pausa);
        else if (Estado == Estado.Pausa) Cambiar(Estado.Jugando);
    }
}
