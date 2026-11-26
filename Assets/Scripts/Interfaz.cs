using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Interfaz del juego, creada por código: marcador, mensajes, menú principal y pausa.
/// Estilo azul marino y dorado. Vive en el objeto Juego durante toda la partida.
/// </summary>
public class Interfaz : MonoBehaviour
{
    static readonly Color Azul = new Color32(0x1d, 0x2a, 0x4d, 0xf2);
    static readonly Color Oro = new Color32(0xd9, 0xa8, 0x4a, 0xff);
    static readonly Color OroClaro = new Color32(0xf3, 0xd6, 0x8a, 0xff);
    static readonly Color Marfil = new Color32(0xf2, 0xe9, 0xd2, 0xff);
    static readonly Color Tinta = new Color32(0x1a, 0x12, 0x05, 0xff);
    static readonly Color Velo = new Color(0.04f, 0.055f, 0.11f, 0.62f);

    Font fuente;
    GameObject velo, marcador, titulo;
    Text textoPergaminos, textoMensaje, textoTitulo, textoAyuda;
    RectTransform menu;
    readonly List<(Image fondo, Text texto, Action accion)> opciones = new List<(Image, Text, Action)>();
    int elegida;
    Alexios alexios;

    void Awake()
    {
        // shortcut: fuente del sistema (Georgia), no existe en Android; incluir un archivo de fuente propio al portar
        fuente = Font.CreateDynamicFontFromOSFont(new[] { "Georgia", "Times New Roman" }, 32);
        if (fuente == null) fuente = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        var lienzo = gameObject.AddComponent<Canvas>();
        lienzo.renderMode = RenderMode.ScreenSpaceOverlay;
        var escala = gameObject.AddComponent<CanvasScaler>();
        escala.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;   // mismo tamaño relativo en cualquier pantalla
        escala.referenceResolution = new Vector2(1920, 1080);
        escala.matchWidthOrHeight = 0.5f;
        gameObject.AddComponent<GraphicRaycaster>();
        gameObject.AddComponent<EventSystem>();
        gameObject.AddComponent<StandaloneInputModule>();   // clics del ratón en las opciones

        // Marcador (arriba a la izquierda) y mensaje debajo
        marcador = Panel("Marcador", transform, new Vector2(0, 1), new Vector2(30, -30), new Vector2(430, 84)).gameObject;
        textoPergaminos = Texto(marcador.transform, "", 40, Marfil, TextAnchor.MiddleCenter);
        textoMensaje = Texto(transform, "", 34, Marfil, TextAnchor.UpperLeft);
        Colocar(textoMensaje.rectTransform, new Vector2(0, 1), new Vector2(36, -130), new Vector2(1300, 60));
        textoMensaje.gameObject.AddComponent<Outline>().effectColor = new Color(0, 0, 0, 0.8f);

        // Velo oscuro, título y lista de opciones (menú principal y pausa)
        velo = new GameObject("Velo", typeof(Image));
        velo.transform.SetParent(transform, false);
        velo.GetComponent<Image>().color = Velo;
        var rv = velo.GetComponent<RectTransform>();
        rv.anchorMin = Vector2.zero; rv.anchorMax = Vector2.one; rv.offsetMin = rv.offsetMax = Vector2.zero;

        titulo = new GameObject("Titulo", typeof(RectTransform));
        titulo.transform.SetParent(transform, false);
        Colocar(titulo.GetComponent<RectTransform>(), new Vector2(0.5f, 1), new Vector2(0, -90), new Vector2(1700, 330));
        textoTitulo = Texto(titulo.transform, "", 110, OroClaro, TextAnchor.MiddleCenter);
        var sombra = textoTitulo.gameObject.AddComponent<Shadow>();
        sombra.effectColor = new Color32(0x5a, 0x3c, 0x10, 0xff);
        sombra.effectDistance = new Vector2(0, -6);

        menu = Panel("Menu", transform, new Vector2(0.5f, 0.5f), new Vector2(0, -150), new Vector2(620, 100));
        textoAyuda = Texto(transform, "", 28, Marfil, TextAnchor.LowerCenter);
        Colocar(textoAyuda.rectTransform, new Vector2(0.5f, 0), new Vector2(0, 30), new Vector2(1700, 50));

        Juego.AlCambiar += Mostrar;
    }

    void OnDestroy() => Juego.AlCambiar -= Mostrar;

    // ---------- Piezas ----------
    static void Colocar(RectTransform r, Vector2 ancla, Vector2 posicion, Vector2 tamano)
    {
        r.anchorMin = r.anchorMax = r.pivot = ancla;
        r.anchoredPosition = posicion;
        r.sizeDelta = tamano;
    }

    RectTransform Panel(string nombre, Transform padre, Vector2 ancla, Vector2 posicion, Vector2 tamano)
    {
        var objeto = new GameObject(nombre, typeof(Image), typeof(Outline));
        objeto.transform.SetParent(padre, false);
        objeto.GetComponent<Image>().color = Azul;
        var borde = objeto.GetComponent<Outline>();
        borde.effectColor = Oro;
        borde.effectDistance = new Vector2(4, 4);
        var r = objeto.GetComponent<RectTransform>();
        Colocar(r, ancla, posicion, tamano);
        return r;
    }

    Text Texto(Transform padre, string contenido, int tamano, Color color, TextAnchor alineacion)
    {
        var objeto = new GameObject("Texto", typeof(Text));
        objeto.transform.SetParent(padre, false);
        var t = objeto.GetComponent<Text>();
        t.font = fuente; t.fontSize = tamano; t.color = color; t.alignment = alineacion; t.text = contenido;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.raycastTarget = false;
        var r = t.rectTransform;   // ocupa todo el padre
        r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.offsetMin = r.offsetMax = Vector2.zero;
        return t;
    }

    // ---------- Pantallas ----------
    void Mostrar(Estado estado)
    {
        alexios = FindFirstObjectByType<Alexios>();
        bool conMenu = estado == Estado.Menu || estado == Estado.Pausa;
        velo.SetActive(conMenu);
        titulo.SetActive(conMenu);
        menu.gameObject.SetActive(conMenu);
        textoAyuda.gameObject.SetActive(conMenu);
        marcador.SetActive(alexios != null && estado != Estado.Menu);
        textoMensaje.gameObject.SetActive(estado == Estado.Jugando);

        if (estado == Estado.Menu)
        {
            textoTitulo.fontSize = 110;
            textoTitulo.text = "LOS HEREDEROS\nDEL OLIMPO";
            var lista = new List<(string, Action)> { ("Nueva partida", Juego.NuevaPartida) };
            if (Guardado.Hay) lista.Insert(0, ("Continuar", Juego.Continuar));
            lista.Add(("Salir", Juego.Salir));
            Opciones(lista);
        }
        else if (estado == Estado.Pausa)
        {
            textoTitulo.fontSize = 80;
            textoTitulo.text = "PAUSA";
            Opciones(new List<(string, Action)>
            {
                ("Seguir jugando", () => Juego.Cambiar(Estado.Jugando)),
                ("Volver al menú", Juego.VolverAlMenu),
            });
        }
    }

    void Opciones(List<(string nombre, Action accion)> lista)
    {
        foreach (var o in opciones) Destroy(o.fondo.gameObject);
        opciones.Clear();
        const float alto = 84f;
        menu.sizeDelta = new Vector2(620, lista.Count * alto + 40);
        for (int i = 0; i < lista.Count; i++)
        {
            var objeto = new GameObject(lista[i].nombre, typeof(Image), typeof(Button));
            objeto.transform.SetParent(menu, false);
            Colocar(objeto.GetComponent<RectTransform>(), new Vector2(0.5f, 1), new Vector2(0, -20 - i * alto), new Vector2(560, alto - 10));
            Action accion = lista[i].accion;
            objeto.GetComponent<Button>().onClick.AddListener(() => accion());
            var texto = Texto(objeto.transform, lista[i].nombre, 44, Marfil, TextAnchor.MiddleCenter);
            opciones.Add((objeto.GetComponent<Image>(), texto, accion));
        }
        elegida = 0;
        textoAyuda.text = "↑ ↓  Elegir      Enter  Aceptar";
        Resaltar();
    }

    void Resaltar()
    {
        for (int i = 0; i < opciones.Count; i++)
        {
            opciones[i].fondo.color = i == elegida ? Oro : Color.clear;
            opciones[i].texto.color = i == elegida ? Tinta : Marfil;
        }
    }

    void Update()
    {
        if (Juego.Estado == Estado.Menu || Juego.Estado == Estado.Pausa)
        {
            if (Juego.AcabaDeCambiar || opciones.Count == 0) return;   // la tecla que abrió el menú no elige nada
            int mover = Controles.MenuVertical;
            if (mover != 0)
            {
                elegida = (elegida + mover + opciones.Count) % opciones.Count;
                Resaltar();
            }
            if (Controles.Aceptar) opciones[elegida].accion();
            return;
        }
        if (alexios == null) return;
        textoPergaminos.text = "Pergaminos  " + alexios.Pergaminos + " / " + alexios.meta;
        textoMensaje.text = alexios.Gano ? alexios.Mensaje + "\nPulsa R para jugar otra vez" : alexios.Mensaje;
    }
}
