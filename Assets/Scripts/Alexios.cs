using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Prólogo – mecánica "Recoger pergaminos".
/// Alexios camina (flechas o A/D), salta (Espacio) y recoge pergaminos.
/// Cuando tiene todos, el santuario se ilumina y al entrar en él gana.
/// Va en el objeto Heroe, junto a Rigidbody 2D y Box Collider 2D.
/// </summary>
public class Alexios : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidad = 5f;
    public float fuerzaSalto = 14f;

    [Header("Meta")]
    public int meta = 3;               // pergaminos necesarios para abrir el santuario
    public SpriteRenderer santuario;   // cambia de color cuando se puede entrar

    [Header("Sonidos")]
    public AudioClip sonidoPergamino;
    public AudioClip sonidoVictoria;

    private Rigidbody2D cuerpo;
    private SpriteRenderer sprite;
    private AudioSource audioFuente;
    private float direccion;
    private bool quiereSaltar;
    private bool enSuelo;
    private int pergaminos = 0;
    private bool gano = false;
    private string mensaje = "Recoge los pergaminos y llévalos al santuario";
    private float finMensaje = 4f;    // momento (Time.time) en que se borra el mensaje
    private float finDestello = 0f;

    // Animaciones (como las de Naia): reposo, caminar, subir, cima, caer y aterrizar
    private Animator[] animadores;     // el de Alexios y el del tridente (objeto hijo)
    private string estadoActual = "reposo";
    private bool estabaEnElAire = false;
    private float finAterrizaje = 0f;

    void Start()
    {
        cuerpo = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        audioFuente = GetComponent<AudioSource>();
        animadores = GetComponentsInChildren<Animator>();
    }

    void Update()
    {
        // Las entradas se leen en Update (cada fotograma) para no perder ninguna pulsación
        direccion = gano ? 0f : Input.GetAxisRaw("Horizontal");
        if (!gano && Input.GetButtonDown("Jump") && enSuelo)
            quiereSaltar = true;

        // Ajuste de mejora: salto variable. Si se suelta Espacio mientras sube, frena:
        // toque corto = salto bajo, mantener = salto alto.
        if (Input.GetButtonUp("Jump") && cuerpo.linearVelocity.y > 0f)
            cuerpo.linearVelocity = new Vector2(cuerpo.linearVelocity.x, cuerpo.linearVelocity.y * 0.5f);

        // El sprite original mira a la derecha; para ir a la izquierda se refleja con la escala X
        if (direccion != 0f)
            transform.localScale = new Vector3(direccion > 0f ? 1f : -1f, 1f, 1f);

        // Destello amarillo al recoger un pergamino
        sprite.color = Time.time < finDestello ? new Color(1f, 0.9f, 0.4f) : Color.white;

        ElegirAnimacion();

        if (gano && Input.GetKeyDown(KeyCode.R))
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Elige la animación según lo que está haciendo Alexios (igual que el salto de Naia:
    // subir, cima, caer y aterrizar). El tridente reproduce el mismo estado para seguir a la mano.
    void ElegirAnimacion()
    {
        float vy = cuerpo.linearVelocity.y;
        if (enSuelo && estabaEnElAire)
            finAterrizaje = Time.time + 0.17f;          // dura lo que la animación de aterrizar
        estabaEnElAire = !enSuelo;

        string estado;
        if (!enSuelo)
            estado = vy > 2f ? "subir" : vy < -2f ? "caer" : "cima";
        else if (Time.time < finAterrizaje)
            estado = "aterrizar";
        else if (direccion != 0f)
            estado = "caminar";
        else
            estado = "reposo";

        if (estado == estadoActual) return;           // solo se cambia cuando hace falta
        estadoActual = estado;
        foreach (Animator animador in animadores)
            animador.Play(estado, 0, 0f);
    }

    void FixedUpdate()
    {
        // La física va en FixedUpdate. Solo se cambia la X: la Y la controla la gravedad
        cuerpo.linearVelocity = new Vector2(direccion * velocidad, cuerpo.linearVelocity.y);
        if (quiereSaltar)
        {
            cuerpo.linearVelocity = new Vector2(cuerpo.linearVelocity.x, fuerzaSalto);
            quiereSaltar = false;
        }
        enSuelo = false; // OnCollisionStay2D lo vuelve a poner en true si sigue pisando algo
    }

    // Regla: solo se salta si se está pisando algo (sin esto, se podría "volar")
    void OnCollisionStay2D(Collision2D choque)
    {
        foreach (ContactPoint2D contacto in choque.contacts)
            if (contacto.normal.y > 0.5f) // la superficie empuja hacia arriba = es suelo
                enSuelo = true;
    }

    void OnTriggerEnter2D(Collider2D otro)
    {
        if (gano) return;

        if (otro.CompareTag("Pergamino"))
        {
            otro.enabled = false;          // evita contar el mismo pergamino dos veces
            Destroy(otro.gameObject);
            pergaminos++;
            audioFuente.PlayOneShot(sonidoPergamino);
            finDestello = Time.time + 0.2f;
            if (pergaminos >= meta)
            {
                santuario.color = new Color(1f, 0.82f, 0.25f); // dorado: ya se puede entrar
                Avisar("¡Tienes todos los pergaminos! Ve al santuario");
            }
            else
                Avisar("Pergamino encontrado");
        }
        else if (otro.CompareTag("Santuario"))
        {
            if (pergaminos >= meta)
            {
                gano = true;
                cuerpo.linearVelocity = Vector2.zero;
                audioFuente.PlayOneShot(sonidoVictoria);
                Avisar("¡El santuario de Atenea se abre!", 999f);
            }
            else
                Avisar("Te faltan " + (meta - pergaminos) + " pergaminos");
        }
    }

    void Avisar(string texto, float segundos = 2.5f)
    {
        mensaje = texto;
        finMensaje = Time.time + segundos;
    }

    void OnGUI()
    {
        GUIStyle estilo = new GUIStyle(GUI.skin.label);
        estilo.fontSize = 24;
        estilo.normal.textColor = Color.white;
        GUI.Label(new Rect(20, 20, 420, 40), "Pergaminos: " + pergaminos + " / " + meta, estilo);
        if (Time.time < finMensaje)
            GUI.Label(new Rect(20, 60, 700, 40), mensaje, estilo);
        if (gano)
            GUI.Label(new Rect(20, 100, 700, 40), "Pulsa R para jugar otra vez", estilo);
    }
}
