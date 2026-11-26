#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;

/// <summary>
/// Construye los recursos del héroe y la escena principal a partir de las hojas
/// limpias del protagonista (Assets/Sprites/Heroe/heroe_*.png + *.json, formato de Aseprite),
/// que exporta Arte/Nuevo/scripts/exportar_unity.py. El arma (la daga) va dibujada en el sprite.
/// Menú: Herramientas > Construir Percy Jackson.
/// Consola: Unity -batchmode -projectPath . -executeMethod ConstruirPercyJackson.Construir -quit
/// </summary>
public static class ConstruirPercyJackson
{
    const string CarpetaSprites = "Assets/Sprites/Heroe";
    const string CarpetaPrefabs = "Assets/Prefabs";
    const string RutaEscena = "Assets/Escenas/Principal.unity";
    const int PixelesPorUnidad = 32;

    // Animaciones del héroe: (nombre, se repite en bucle). La primera es la de inicio.
    static readonly (string, bool)[] Animaciones =
    {
        ("reposo", true), ("caminar", true), ("correr", true), ("subir", false), ("cima", true), ("caer", true),
        ("aterrizar", false), ("ataque", false), ("especial", false), ("defender", false), ("danio", false),
        ("interactuar", false), ("recoger", false), ("leer", false), ("derrota", false),
    };

    static readonly Color ColorFondo = new Color(0.10f, 0.13f, 0.19f);

    // Formato JSON de Aseprite (json-array)
    [System.Serializable] class Rectangulo { public int x, y, w, h; }
    [System.Serializable] class Tamano { public int w, h; }
    [System.Serializable] class Fotograma { public Rectangulo frame; public int duration; }
    [System.Serializable] class Meta { public Tamano size; }
    [System.Serializable] class Hoja { public Fotograma[] frames; public Meta meta; }

    [MenuItem("Herramientas/Construir Percy Jackson")]
    public static void Construir()
    {
        EditorSettings.defaultBehaviorMode = EditorBehaviorMode.Mode2D;
        foreach (var c in new[] { "Assets/Animaciones", "Assets/Animaciones/Heroe", CarpetaPrefabs, "Assets/Escenas" })
            CrearCarpeta(c);
        AssetDatabase.Refresh();

        // Un estado por animación; Alexios.cs elige cuál se reproduce por su nombre
        var estados = Animaciones.Select(a => (a.Item1,
            CrearClip("heroe_" + a.Item1, $"Assets/Animaciones/Heroe/{a.Item1}.anim", a.Item2))).ToList();
        var ctrlHeroe = CrearControlador("Assets/Animaciones/Heroe/Heroe.controller", estados);

        var prefab = CrearPrefab(ctrlHeroe);
        CrearEscena(prefab);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(RutaEscena, true) };
        AssetDatabase.SaveAssets();
        Verificar();
        Debug.Log("[PercyJackson] Escena construida en " + RutaEscena);
    }

    static void CrearCarpeta(string ruta)
    {
        if (!AssetDatabase.IsValidFolder(ruta))
            AssetDatabase.CreateFolder(Path.GetDirectoryName(ruta).Replace('\\', '/'), Path.GetFileName(ruta));
    }

    // ---------- Sprites ----------
    static void ConfigurarImportador(TextureImporter ti, SpriteImportMode modo)
    {
        ti.textureType = TextureImporterType.Sprite;
        ti.spriteImportMode = modo;
        ti.spritePixelsPerUnit = PixelesPorUnidad;
        ti.filterMode = FilterMode.Point;              // pixel art nítido
        ti.textureCompression = TextureImporterCompression.Uncompressed;
        ti.mipmapEnabled = false;
        ti.alphaIsTransparency = true;
    }

    /// Corta la hoja en un sprite por fotograma, con el pivote en los pies.
    static void CortarHoja(string png, string prefijo, Hoja datos)
    {
        var ti = (TextureImporter)AssetImporter.GetAtPath(png);
        ConfigurarImportador(ti, SpriteImportMode.Multiple);
        var fabrica = new SpriteDataProviderFactories();
        fabrica.Init();
        var proveedor = fabrica.GetSpriteEditorDataProviderFromObject(ti);
        proveedor.InitSpriteEditorDataProvider();

        int alto = datos.meta.size.h;
        var rects = datos.frames.Select((f, i) => new SpriteRect
        {
            name = $"{prefijo}_{i:00}",
            spriteID = GUID.Generate(),
            // Aseprite cuenta desde arriba; Unity, desde abajo
            rect = new Rect(f.frame.x, alto - f.frame.y - f.frame.h, f.frame.w, f.frame.h),
            alignment = SpriteAlignment.BottomCenter,
            pivot = new Vector2(0.5f, 0f),
        }).ToArray();
        proveedor.SetSpriteRects(rects);
        proveedor.GetDataProvider<ISpriteNameFileIdDataProvider>()?
            .SetNameFileIdPairs(rects.Select(r => new SpriteNameFileIdPair(r.name, r.spriteID)));
        proveedor.Apply();
        ti.SaveAndReimport();
    }

    static Sprite[] Sprites(string png) =>
        AssetDatabase.LoadAllAssetsAtPath(png).OfType<Sprite>().OrderBy(s => s.name).ToArray();

    // ---------- Animaciones ----------
    static AnimationClip CrearClip(string hoja, string ruta, bool bucle = true)
    {
        string png = $"{CarpetaSprites}/{hoja}.png";
        var datos = JsonUtility.FromJson<Hoja>(File.ReadAllText($"{CarpetaSprites}/{hoja}.json"));
        CortarHoja(png, hoja, datos);
        var sprites = Sprites(png);

        var claves = new ObjectReferenceKeyframe[sprites.Length + 1];
        float t = 0f;
        for (int i = 0; i < sprites.Length; i++)
        {
            claves[i] = new ObjectReferenceKeyframe { time = t, value = sprites[i] };
            t += datos.frames[i].duration / 1000f;
        }
        // Clave final: el último fotograma dura lo que indica Aseprite
        claves[sprites.Length] = new ObjectReferenceKeyframe { time = t, value = sprites[sprites.Length - 1] };

        // 100 fps internos: las duraciones de Aseprite son múltiplos de 10 ms
        var clip = new AnimationClip { frameRate = 100f };
        AnimationUtility.SetObjectReferenceCurve(clip,
            EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite"), claves);
        var ajustes = AnimationUtility.GetAnimationClipSettings(clip);
        ajustes.loopTime = bucle;
        AnimationUtility.SetAnimationClipSettings(clip, ajustes);

        AssetDatabase.DeleteAsset(ruta);
        AssetDatabase.CreateAsset(clip, ruta);
        return clip;
    }

    // Un estado por animación; el primero es el de inicio. Sin transiciones: el script elige el estado.
    static AnimatorController CrearControlador(string ruta, System.Collections.Generic.List<(string, AnimationClip)> estados)
    {
        AssetDatabase.DeleteAsset(ruta);
        var ctrl = AnimatorController.CreateAnimatorControllerAtPath(ruta);
        var maquina = ctrl.layers[0].stateMachine;
        foreach (var (nombre, clip) in estados)
            maquina.AddState(nombre).motion = clip;
        maquina.defaultState = maquina.states[0].state;
        return ctrl;
    }

    // ---------- Prefab y escena ----------
    static GameObject CrearPrefab(AnimatorController ctrlHeroe)
    {
        var heroe = new GameObject("Heroe");
        heroe.AddComponent<SpriteRenderer>().sprite = Sprites(CarpetaSprites + "/heroe_reposo.png")[0];
        heroe.AddComponent<Animator>().runtimeAnimatorController = ctrlHeroe;

        var prefab = PrefabUtility.SaveAsPrefabAsset(heroe, CarpetaPrefabs + "/Heroe.prefab");
        Object.DestroyImmediate(heroe);
        return prefab;
    }

    static void CrearEscena(GameObject prefab)
    {
        var escena = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var camGO = new GameObject("Main Camera");
        camGO.tag = "MainCamera";
        var cam = camGO.AddComponent<Camera>();
        camGO.AddComponent<AudioListener>();
        camGO.transform.position = new Vector3(0, 0, -10);
        cam.orthographic = true;
        cam.orthographicSize = 3f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = ColorFondo;

        var heroe = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        heroe.transform.position = new Vector3(0f, -1.1f, 0f);

        EditorSceneManager.SaveScene(escena, RutaEscena);
    }

    // ---------- Comprobación ----------
    static void Verificar()
    {
        void Chk(bool ok, string que) => Debug.Log((ok ? "[OK] " : "[FALLO] ") + que);
        var esperados = new System.Collections.Generic.Dictionary<string, int>
        {
            ["reposo"] = 1, ["caminar"] = 8, ["correr"] = 8, ["subir"] = 2, ["cima"] = 1, ["caer"] = 1, ["aterrizar"] = 2,
            ["ataque"] = 6, ["especial"] = 6, ["defender"] = 4, ["danio"] = 4, ["interactuar"] = 4, ["recoger"] = 4,
            ["leer"] = 4, ["derrota"] = 4,
        };
        foreach (var e in esperados)
            Chk(Sprites($"{CarpetaSprites}/heroe_{e.Key}.png").Length == e.Value, $"heroe_{e.Key}: {e.Value} sprites");
        var caminar = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Animaciones/Heroe/caminar.anim");
        Chk(caminar.isLooping && Mathf.Abs(caminar.length - 0.64f) < 0.015f, $"caminar en bucle, {caminar.length:0.00} s");
        var ataque = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Animaciones/Heroe/ataque.anim");
        Chk(!ataque.isLooping && Mathf.Abs(ataque.length - 0.42f) < 0.015f, $"ataque sin bucle, {ataque.length:0.00} s");
        var aterrizar = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Animaciones/Heroe/aterrizar.anim");
        Chk(!aterrizar.isLooping && Mathf.Abs(aterrizar.length - 0.17f) < 0.015f, $"aterrizar sin bucle, {aterrizar.length:0.00} s");
        var ctrl = AssetDatabase.LoadAssetAtPath<AnimatorController>("Assets/Animaciones/Heroe/Heroe.controller");
        Chk(ctrl.layers[0].stateMachine.states.Length == Animaciones.Length && ctrl.layers[0].stateMachine.defaultState.name == "reposo",
            $"Heroe.controller: {Animaciones.Length} estados, empieza en reposo");
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CarpetaPrefabs + "/Heroe.prefab");
        Chk(prefab != null && prefab.transform.childCount == 0, "prefab Heroe sin tridente aparte");
        Chk(File.Exists(RutaEscena), "escena principal guardada");
    }
}
#endif
