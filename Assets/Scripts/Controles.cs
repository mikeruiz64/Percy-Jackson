using UnityEngine;

/// <summary>
/// Todas las entradas del jugador en un solo sitio: teclado y mando (disposición de Xbox en Windows).
/// El resto del juego pregunta aquí y no mira teclas sueltas.
/// </summary>
public static class Controles
{
    // Botones del mando: A = 0, X = 2, Y = 3, RB = 5, Select = 6, Start = 7
    const KeyCode MandoA = KeyCode.JoystickButton0, MandoX = KeyCode.JoystickButton2,
                  MandoY = KeyCode.JoystickButton3, MandoRB = KeyCode.JoystickButton5,
                  MandoSelect = KeyCode.JoystickButton6, MandoStart = KeyCode.JoystickButton7;

    static float verticalAnterior;
    static int fotogramaVertical = -1, menuVertical;

    public static float Horizontal => Input.GetAxisRaw("Horizontal");   // flechas, A/D y palanca
    public static bool Correr => Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) || Input.GetKey(MandoRB);
    public static bool SaltarPulsado => Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(MandoA);
    public static bool SaltarSoltado => Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(MandoA);
    public static bool Atacar => Input.GetKeyDown(KeyCode.J) || Input.GetKeyDown(MandoX);
    public static bool Interactuar => Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(MandoY);
    public static bool Codice => Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(MandoSelect);
    public static bool Pausa => Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(MandoStart);

    // Menús
    public static bool Aceptar => Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)
                               || Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(MandoA);

    /// −1 al pulsar arriba, +1 al pulsar abajo, 0 si no. La palanca cuenta una vez por empujón.
    public static int MenuVertical
    {
        get
        {
            if (fotogramaVertical == Time.frameCount) return menuVertical;   // mismo valor para todo el fotograma
            fotogramaVertical = Time.frameCount;
            float v = Input.GetAxisRaw("Vertical");
            menuVertical = 0;
            if (v > 0.5f && verticalAnterior <= 0.5f) menuVertical = -1;
            else if (v < -0.5f && verticalAnterior >= -0.5f) menuVertical = 1;
            verticalAnterior = v;
            return menuVertical;
        }
    }
}
