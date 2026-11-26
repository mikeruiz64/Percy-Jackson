#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Construye la escena del Prólogo (calle de Atenas) con la mecánica "Recoger pergaminos".
/// Menú: Herramientas > Construir Prólogo.
/// Consola: Unity -batchmode -projectPath . -executeMethod ConstruirPrologo.Construir -quit
/// Necesita antes Herramientas > Construir Percy Jackson (usa el prefab Heroe).
/// </summary>
public static class ConstruirPrologo
{
    const string RutaEscena = "Assets/Escenas/Prologo.unity";
    const string CarpetaSonidos = "Assets/Sonidos";
    const string CarpetaMapa = "Assets/Sprites/Mapa";   // arte exportado por Arte/Mapa/exportar_mapa.py

    static readonly Color Cielo = new Color32(0x2b, 0x3a, 0x67, 0xff);  // color más alto del cielo del amanecer
    static readonly Color SantuarioCerrado = new Color(0.72f, 0.72f, 0.78f); // apagado; dorado al abrirse

    // Pivotes guardados por exportar_mapa.py (mapa.json)
    [System.Serializable] class Pivotes { public float[] nivel, pergamino, santuario; }

    [MenuItem("Herramientas/Construir Prólogo")]
    public static void Construir()
    {
        Directory.CreateDirectory("Assets/Fisica");
        Directory.CreateDirectory(CarpetaSonidos);
        EscribirSonido(CarpetaSonidos + "/pergamino.wav", new[] { 880f, 1320f }, 0.08f);
        EscribirSonido(CarpetaSonidos + "/victoria.wav", new[] { 523f, 659f, 784f, 1047f }, 0.15f);
        AssetDatabase.Refresh();
        CrearEtiqueta("Pergamino");
        CrearEtiqueta("Santuario");

        var pivotes = JsonUtility.FromJson<Pivotes>(File.ReadAllText(CarpetaMapa + "/mapa.json"));
        var spriteNivel = ImportarSprite("nivel", pivotes.nivel);
        var spritePergamino = ImportarSprite("pergamino", pivotes.pergamino);
        var spriteSantuario = ImportarSprite("santuario", pivotes.santuario);
        var escena = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Cámara ortográfica, tamaño 5 (como en la guía); además sigue a Alexios
        var camGO = new GameObject("Main Camera") { tag = "MainCamera" };
        var cam = camGO.AddComponent<Camera>();
        camGO.AddComponent<AudioListener>();
        camGO.transform.position = new Vector3(0, 0, -10);
        cam.orthographic = true;
        cam.orthographicSize = 5f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Cielo;

        // Fondo: una sola imagen con el cielo, el Partenón y la calle (pivote en la esquina inferior izquierda)
        var fondo = new GameObject("Fondo");
        fondo.transform.position = new Vector3(-9.4f, -5f, 0f);
        var srFondo = fondo.AddComponent<SpriteRenderer>();
        srFondo.sprite = spriteNivel;
        srFondo.sortingOrder = -10; // detrás de todo

        // Mapa "Calle hacia la colina" (x de -9 a 33). Medidas = esquina inferior izquierda, ancho, alto.
        // Suelo, casas, columnas y colina: colliders sólidos e invisibles (el dibujo está en el Fondo)
        Pieza("Calle", null, -9f, -5f, 19f, 1f, false);
        Pieza("Escalon1", null, 10f, -5f, 8f, 2.5f, false);
        Pieza("Escalon2", null, 18f, -5f, 6f, 4f, false);
        Pieza("Colina", null, 24f, -5f, 9f, 5.5f, false);
        Pieza("Casa", null, 1f, -4f, 4f, 2.5f, false);
        Pieza("Anfora", null, 7f, -4f, 0.8f, 1.2f, false);
        Pieza("Columna1", null, 12.5f, -2.5f, 0.6f, 2.3f, false);
        Pieza("Columna2", null, 15.5f, -2.5f, 0.6f, 2.3f, false);
        Pieza("Dintel", null, 12.2f, -0.2f, 4.2f, 0.4f, false);
        Pieza("ParedIzquierda", null, -9.4f, -5f, 0.4f, 12f, false);
        Pieza("ParedDerecha", null, 33f, -5f, 0.4f, 12f, false);

        // Pergaminos: trigger + etiqueta, como las monedas de la guía (tejado, dintel y colina)
        var p = new[] { new Vector2(2.8f, -1.3f), new Vector2(14.2f, 0.4f), new Vector2(21f, 1.4f) };
        for (int i = 0; i < p.Length; i++)
            Pieza("Pergamino" + (i + 1), spritePergamino, p[i].x, p[i].y, 0.4f, 0.55f, true).tag = "Pergamino";

        // Santuario en lo alto de la colina: trigger. Apagado = cerrado; Alexios lo pinta de dorado al tener la meta
        var santuario = Pieza("Santuario", spriteSantuario, 28.5f, 0.5f, 1.8f, 2.8f, true);
        santuario.tag = "Santuario";
        santuario.GetComponent<SpriteRenderer>().color = SantuarioCerrado;

        // Puntos de control (invisibles por ahora): al pasar se guarda la partida
        Pieza("PuntoControl1", null, 10.4f, -2.5f, 0.6f, 3f, true).AddComponent<PuntoControl>();
        Pieza("PuntoControl2", null, 24.4f, 0.5f, 0.6f, 3f, true).AddComponent<PuntoControl>();

        // Alexios: prefab del héroe + física + script de la mecánica
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Heroe.prefab");
        var heroe = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        heroe.name = "Alexios";
        heroe.transform.position = new Vector3(-6.6f, -4f, 0f);
        var cuerpo = heroe.AddComponent<Rigidbody2D>();
        cuerpo.gravityScale = 3f;            // caída más rápida: el salto se siente menos "flotante"
        cuerpo.freezeRotation = true;        // que no gire al chocar
        cuerpo.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        var caja = heroe.AddComponent<BoxCollider2D>();
        caja.size = new Vector2(0.8f, 2f);   // el sprite mide 1,4 × 2,2 unidades; la caja es más estrecha
        caja.offset = new Vector2(0f, 1f);   // el pivote está en los pies
        caja.sharedMaterial = SinFriccion();  // sin esto se queda pegado a las paredes al saltar contra ellas
        heroe.AddComponent<AudioSource>().playOnAwake = false;
        var alexios = heroe.AddComponent<Alexios>();
        alexios.santuario = santuario.GetComponent<SpriteRenderer>();
        alexios.sonidoPergamino = AssetDatabase.LoadAssetAtPath<AudioClip>(CarpetaSonidos + "/pergamino.wav");
        alexios.sonidoVictoria = AssetDatabase.LoadAssetAtPath<AudioClip>(CarpetaSonidos + "/victoria.wav");

        camGO.AddComponent<CamaraSigue>().objetivo = heroe.transform;

        EditorSceneManager.SaveScene(escena, RutaEscena);
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(RutaEscena, true),
            new EditorBuildSettingsScene("Assets/Escenas/Principal.unity", true),
        };
        AssetDatabase.SaveAssets();
        Verificar(alexios);
    }

    // Material de física con fricción 0: Alexios se desliza por las paredes en vez de engancharse
    static PhysicsMaterial2D SinFriccion()
    {
        const string ruta = "Assets/Fisica/SinFriccion.physicsMaterial2D";
        var m = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(ruta);
        if (m == null)
        {
            m = new PhysicsMaterial2D("SinFriccion") { friction = 0f, bounciness = 0f };
            AssetDatabase.CreateAsset(m, ruta);
        }
        return m;
    }

    // x, y = esquina inferior izquierda (como en el plano del mapa); Unity coloca por el centro.
    // Sin sprite: collider invisible escalado a la medida. Con sprite: el dibujo manda y el collider se ajusta.
    static GameObject Pieza(string nombre, Sprite sprite, float x, float y, float ancho, float alto, bool trigger)
    {
        var go = new GameObject(nombre);
        go.transform.position = new Vector2(x + ancho / 2f, y + alto / 2f);
        var caja = go.AddComponent<BoxCollider2D>();
        caja.isTrigger = trigger;
        if (sprite == null)
            go.transform.localScale = new Vector3(ancho, alto, 1f); // la caja mide 1 × 1 y se escala
        else
        {
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = -1; // detrás del héroe
            caja.size = new Vector2(ancho, alto);
        }
        return go;
    }

    // Pixel art nítido a 32 px por unidad (igual que Alexios), con el pivote que calculó la exportación
    static Sprite ImportarSprite(string nombre, float[] pivote)
    {
        string ruta = CarpetaMapa + "/" + nombre + ".png";
        var ti = (TextureImporter)AssetImporter.GetAtPath(ruta);
        ti.textureType = TextureImporterType.Sprite;
        ti.spriteImportMode = SpriteImportMode.Single;
        ti.spritePixelsPerUnit = 32;
        ti.filterMode = FilterMode.Point;
        ti.textureCompression = TextureImporterCompression.Uncompressed;
        ti.mipmapEnabled = false;
        var ajustes = new TextureImporterSettings();
        ti.ReadTextureSettings(ajustes);
        ajustes.spriteAlignment = (int)SpriteAlignment.Custom;
        ajustes.spritePivot = new Vector2(pivote[0], pivote[1]);
        ti.SetTextureSettings(ajustes);
        ti.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(ruta);
    }

    // WAV de 16 bits mono: una nota corta por frecuencia, con caída de volumen
    static void EscribirSonido(string ruta, float[] notas, float durNota)
    {
        const int hz = 22050;
        int porNota = (int)(hz * durNota);
        var muestras = new short[porNota * notas.Length];
        for (int n = 0; n < notas.Length; n++)
            for (int i = 0; i < porNota; i++)
            {
                float t = (float)i / hz, caida = 1f - (float)i / porNota;
                muestras[n * porNota + i] = (short)(Mathf.Sin(2 * Mathf.PI * notas[n] * t) * caida * 9000);
            }
        using var w = new BinaryWriter(File.Create(ruta));
        w.Write("RIFF".ToCharArray()); w.Write(36 + muestras.Length * 2); w.Write("WAVEfmt ".ToCharArray());
        w.Write(16); w.Write((short)1); w.Write((short)1); w.Write(hz); w.Write(hz * 2); w.Write((short)2); w.Write((short)16);
        w.Write("data".ToCharArray()); w.Write(muestras.Length * 2);
        foreach (var m in muestras) w.Write(m);
    }

    static void CrearEtiqueta(string etiqueta)
    {
        var tm = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        var tags = tm.FindProperty("tags");
        for (int i = 0; i < tags.arraySize; i++)
            if (tags.GetArrayElementAtIndex(i).stringValue == etiqueta) return;
        tags.InsertArrayElementAtIndex(tags.arraySize);
        tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = etiqueta;
        tm.ApplyModifiedPropertiesWithoutUndo();
    }

    static void Verificar(Alexios a)
    {
        void Chk(bool ok, string que) => Debug.Log((ok ? "[OK] " : "[FALLO] ") + que);
        Chk(GameObject.FindGameObjectsWithTag("Pergamino").Length == a.meta, "3 pergaminos con etiqueta y meta = 3");
        Chk(GameObject.FindWithTag("Santuario") != null, "santuario con etiqueta");
        Chk(a.sonidoPergamino != null && a.sonidoVictoria != null, "sonidos cargados");
        Chk(a.GetComponent<Rigidbody2D>() != null && a.GetComponent<BoxCollider2D>() != null, "Alexios con Rigidbody 2D y Box Collider 2D");
        Chk(Camera.main.GetComponent<CamaraSigue>().objetivo == a.transform, "la cámara sigue a Alexios");
        Chk(GameObject.Find("Fondo").GetComponent<SpriteRenderer>().sprite != null, "fondo del nivel con su imagen");
        Chk(GameObject.Find("Santuario").GetComponent<SpriteRenderer>().sprite != null, "santuario con su sprite");
        Chk(Object.FindObjectsByType<PuntoControl>(FindObjectsSortMode.None).Length == 2, "2 puntos de control");
        var partida = new Guardado.Partida { nivel = "Prologo", x = 1.5f };
        partida.pergaminos.Add("Pergamino2");
        var leida = JsonUtility.FromJson<Guardado.Partida>(JsonUtility.ToJson(partida));
        Chk(leida.nivel == "Prologo" && leida.x == 1.5f && leida.pergaminos.Count == 1 && leida.pergaminos[0] == "Pergamino2",
            "la partida se guarda y se lee igual");
        Chk(File.Exists(RutaEscena), "escena Prologo guardada");
    }
}
#endif
