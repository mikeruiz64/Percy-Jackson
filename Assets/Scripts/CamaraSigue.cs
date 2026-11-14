using UnityEngine;

/// <summary>
/// La cámara sigue a Alexios de izquierda a derecha, sin salirse del nivel.
/// Va en la Main Camera.
/// </summary>
public class CamaraSigue : MonoBehaviour
{
    public Transform objetivo;   // Alexios
    public float minX = -0.3f;   // límites para no enseñar lo que hay fuera de las paredes
    public float maxX = 24.1f;

    // LateUpdate va después de que Alexios se mueva: así la cámara no tiembla
    void LateUpdate()
    {
        float x = Mathf.Clamp(objetivo.position.x, minX, maxX);
        transform.position = new Vector3(x, transform.position.y, transform.position.z);
    }
}
