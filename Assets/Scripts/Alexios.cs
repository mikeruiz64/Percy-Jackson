using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Prólogo – mecánica "Recoger pergaminos".
/// Alexios camina, corre, salta, ataca con la daga y recoge pergaminos (las teclas están en Controles).
/// Cuando tiene todos, el santuario se ilumina y al entrar en él gana.
/// Va en el objeto Heroe, junto a Rigidbody 2D y Box Collider 2D.
/// </summary>
public class Alexios : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidad = 5f;
    public float velocidadCorrer = 9f;
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

    // Animaciones: reposo, caminar, correr, subir, cima, caer, aterrizar y ataque
    private Animator animador;
    private bool corre;
    private float finAtaque = 0f;
    private string estadoActual = "reposo";
    private bool estabaEnElAire = false;
    private float finAterrizaje = 0f;

    static readonly Color Dorado = new Color(1f, 0.82f, 0.25f);   // santuario abierto

    // Lo que enseña la interfaz
    public int Pergaminos => pergaminos;
    public bool Gano => gano;
    public string Mensaje => Time.time < finMensaje ? mensaje : "";

    void Start()
    {
        cuerpo = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        audioFuente = GetComponent<AudioSource>();
        animador = GetComponent<Animator>();
        // Un cuerpo dormido deja de recibir OnCollisionStay2D y Alexios creería que está en el aire
        cuerpo.sleepMode = RigidbodySleepMode2D.NeverSleep;
        RecuperarPartida();
    }

    // Al continuar una partida: vuelve al punto de control y no repite los pergaminos ya recogidos
    void RecuperarPartida()
    {
        Guardado.Partida partida = Guardado.Datos;
        if (partida.nivel != SceneManager.GetActiveScene().name) return;
        transform.position = new Vector3(partida.x, partida.y, transform.position.z);
        foreach (GameObject pergamino in GameObject.FindGameObjectsWithTag("Pergamino"))
            if (partida.pergaminos.Contains(pergamino.name))
            {
                Destroy(pergamino);
                pergaminos++;
            }
        if (pergaminos >= meta) santuario.color = Dorado;
        Avisar("Partida recuperada");
    }

    void Update()
    {
        // Con un menú, un diálogo o la pausa abiertos, Alexios no recibe órdenes
        if (Juego.Estado != Estado.Jugando || Juego.AcabaDeCambiar)
        {
            direccion = 0f;
            return;
        }

        // Las entradas se leen en Update (cada fotograma) para no perder ninguna pulsación
        direccion = gano ? 0f : Controles.Horizontal;
        if (!gano && Controles.SaltarPulsado && enSuelo)
            quiereSaltar = true;
        corre = Controles.Correr;
        if (!gano && Controles.Atacar && enSuelo && Time.time >= finAtaque)
            finAtaque = Time.time + 0.42f;              // dura lo que la animación de ataque

        // Ajuste de mejora: salto variable. Si se suelta el salto mientras sube, frena:
        // toque corto = salto bajo, mantener = salto alto.
        if (Controles.SaltarSoltado && cuerpo.linearVelocity.y > 0f)
            cuerpo.linearVelocity = new Vector2(cuerpo.linearVelocity.x, cuerpo.linearVelocity.y * 0.5f);

        // El sprite original mira a la derecha; para ir a la izquierda se refleja con la escala X
        if (direccion != 0f)
            transform.localScale = new Vector3(direccion > 0f ? 1f : -1f, 1f, 1f);

        // Destello amarillo al recoger un pergamino
        sprite.color = Time.time < finDestello ? new Color(1f, 0.9f, 0.4f) : Color.white;

        ElegirAnimacion();

        if (gano && Input.GetKeyDown(KeyCode.R))
            Juego.NuevaPartida();
    }

    // Elige la animación según lo que está haciendo Alexios (el salto va por fases:
    // subir, cima, caer y aterrizar).
    void ElegirAnimacion()
    {
        float vy = cuerpo.linearVelocity.y;
        if (enSuelo && estabaEnElAire)
            finAterrizaje = Time.time + 0.17f;          // dura lo que la animación de aterrizar
        estabaEnElAire = !enSuelo;

        string estado;
        if (Time.time < finAtaque)
            estado = "ataque";
        else if (!enSuelo)
            estado = vy > 2f ? "subir" : vy < -2f ? "caer" : "cima";
        else if (Time.time < finAterrizaje)
            estado = "aterrizar";
        else if (direccion != 0f)
            estado = corre ? "correr" : "caminar";
        else
            estado = "reposo";

        if (estado == estadoActual) return;           // solo se cambia cuando hace falta
        estadoActual = estado;
        animador.Play(estado, 0, 0f);
    }

    void FixedUpdate()
    {
        // La física va en FixedUpdate. Solo se cambia la X: la Y la controla la gravedad
        // Mientras ataca en el suelo se queda quieto
        float vx = Time.time < finAtaque ? 0f : direccion * (corre ? velocidadCorrer : velocidad);
        cuerpo.linearVelocity = new Vector2(vx, cuerpo.linearVelocity.y);
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
            Guardado.Datos.pergaminos.Add(otro.name);   // queda en la partida al llegar a un punto de control
            Destroy(otro.gameObject);
            pergaminos++;
            audioFuente.PlayOneShot(sonidoPergamino);
            finDestello = Time.time + 0.2f;
            if (pergaminos >= meta)
            {
                santuario.color = Dorado;   // dorado: ya se puede entrar
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

    public void Avisar(string texto, float segundos = 2.5f)
    {
        mensaje = texto;
        finMensaje = Time.time + segundos;
    }
}
