using UnityEngine;

/// <summary>
/// La cámara sigue a Alexios de izquierda a derecha con suavidad, sin salirse del nivel.
/// Va en la Main Camera.
/// </summary>
public class CamaraSigue : MonoBehaviour
{
    public Transform objetivo;   // Alexios
    public float minX = -0.3f;   // límites para no enseñar lo que hay fuera de las paredes
    public float maxX = 24.1f;
    public float suavizado = 0.15f;   // segundos que tarda en alcanzarlo

    float velocidadX;

    // Al empezar (o al continuar una partida) se coloca ya sobre Alexios, sin barrido
    void Start()
    {
        float x = Mathf.Clamp(objetivo.position.x, minX, maxX);
        transform.position = new Vector3(x, transform.position.y, transform.position.z);
    }

    // LateUpdate va después de que Alexios se mueva: así la cámara no tiembla
    void LateUpdate()
    {
        if (Time.deltaTime <= 0f) return;   // en pausa no se mueve
        float destino = Mathf.Clamp(objetivo.position.x, minX, maxX);
        float x = Mathf.SmoothDamp(transform.position.x, destino, ref velocidadX, suavizado);
        transform.position = new Vector3(x, transform.position.y, transform.position.z);
    }
}
