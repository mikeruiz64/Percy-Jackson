using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Punto de control: al tocarlo Alexios se guarda la partida en ese sitio.
/// Va en un objeto con un collider marcado como trigger.
/// </summary>
public class PuntoControl : MonoBehaviour
{
    bool usado;

    void OnTriggerEnter2D(Collider2D otro)
    {
        if (usado || !otro.TryGetComponent(out Alexios alexios)) return;
        usado = true;   // cada punto guarda una sola vez por visita
        Guardado.Datos.nivel = SceneManager.GetActiveScene().name;
        Guardado.Datos.x = transform.position.x;
        Guardado.Datos.y = GetComponent<Collider2D>().bounds.min.y;   // la base: Alexios reaparece de pie en el suelo
        Guardado.Guardar();
        alexios.Avisar("Partida guardada");
    }
}
