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
/// exportadas desde Aseprite (Assets/Sprites/Heroe/*.png + *.json).
/// Menú: Herramientas > Construir Percy Jackson.
/// Consola: Unity -batchmode -projectPath . -executeMethod ConstruirPercyJackson.Construir -quit
/// </summary>
public static class ConstruirPercyJackson
{
    const string CarpetaSprites = "Assets/Sprites/Heroe";
    const string CarpetaPrefabs = "Assets/Prefabs";
    const string RutaEscena = "Assets/Escenas/Principal.unity";
    const int PixelesPorUnidad = 32;

    // Animaciones de movimiento: (nombre, se repite en bucle)
    static readonly (string, bool)[] Movimiento =
        { ("caminar", true), ("subir", false), ("cima", true), ("caer", true), ("aterrizar", false) };

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
        foreach (var c in new[] { "Assets/Animaciones", "Assets/Animaciones/Heroe",
                                  "Assets/Animaciones/Tridente", CarpetaPrefabs, "Assets/Escenas" })
            CrearCarpeta(c);
        AssetDatabase.Refresh();

        var reposo = CrearClip("heroe_reposo", "Assets/Animaciones/Heroe/reposo.anim");
        var brillo = CrearClip("tridente_brillo", "Assets/Animaciones/Tridente/brillo.anim");
        // Caminar y saltar (como Naia). El héroe y el tridente tienen estados con el mismo nombre,
        // así Alexios.cs reproduce el mismo estado en los dos y el tridente sigue a la mano.
        var estadosHeroe = new System.Collections.Generic.List<(string, AnimationClip)> { ("reposo", reposo) };
        var estadosTridente = new System.Collections.Generic.List<(string, AnimationClip)> { ("reposo", brillo) };
        foreach (var (nombre, bucle) in Movimiento)
        {
            estadosHeroe.Add((nombre, CrearClip("heroe_" + nombre, $"Assets/Animaciones/Heroe/{nombre}.anim", bucle)));
            estadosTridente.Add((nombre, CrearClip("tridente_" + nombre, $"Assets/Animaciones/Tridente/{nombre}.anim", bucle)));
        }
        var ctrlHeroe = CrearControlador("Assets/Animaciones/Heroe/Heroe.controller", estadosHeroe);
        var ctrlTridente = CrearControlador("Assets/Animaciones/Tridente/Tridente.controller", estadosTridente);

        var suelto = (TextureImporter)AssetImporter.GetAtPath(CarpetaSprites + "/tridente.png");
        ConfigurarImportador(suelto, SpriteImportMode.Single);
        suelto.SaveAndReimport();

        var prefab = CrearPrefab(ctrlHeroe, ctrlTridente);
        CrearEscena(prefab);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(RutaEscena, true) };
        AssetDatabase.SaveAssets();
        Verificar(reposo, brillo);
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
    static GameObject CrearPrefab(AnimatorController ctrlHeroe, AnimatorController ctrlTridente)
    {
        var heroe = new GameObject("Heroe");
        heroe.AddComponent<SpriteRenderer>().sprite = Sprites(CarpetaSprites + "/heroe_reposo.png")[0];
        heroe.AddComponent<Animator>().runtimeAnimatorController = ctrlHeroe;

        // El tridente va encima del personaje, como en el sprite original
        var tridente = new GameObject("Tridente");
        tridente.transform.SetParent(heroe.transform, false);
        var srTridente = tridente.AddComponent<SpriteRenderer>();
        srTridente.sprite = Sprites(CarpetaSprites + "/tridente_brillo.png")[0];
        srTridente.sortingOrder = 1;
        tridente.AddComponent<Animator>().runtimeAnimatorController = ctrlTridente;

        var componente = new SerializedObject(heroe.AddComponent<Heroe>());
        componente.FindProperty("tridente").objectReferenceValue = tridente;
        componente.ApplyModifiedPropertiesWithoutUndo();

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
    static void Verificar(AnimationClip reposo, AnimationClip brillo)
    {
        void Chk(bool ok, string que) => Debug.Log((ok ? "[OK] " : "[FALLO] ") + que);
        Chk(Sprites(CarpetaSprites + "/heroe_reposo.png").Length == 4, "heroe_reposo: 4 sprites");
        Chk(Sprites(CarpetaSprites + "/tridente_brillo.png").Length == 6, "tridente_brillo: 6 sprites");
        Chk(reposo.isLooping && Mathf.Abs(reposo.length - 1.28f) < 0.01f, $"reposo en bucle, {reposo.length:0.00} s");
        Chk(brillo.isLooping && Mathf.Abs(brillo.length - 2.03f) < 0.01f, $"brillo en bucle, {brillo.length:0.00} s");
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CarpetaPrefabs + "/Heroe.prefab");
        Chk(prefab != null && prefab.transform.Find("Tridente") != null, "prefab Heroe con el tridente como hijo");
        Chk(File.Exists(RutaEscena), "escena principal guardada");
        Chk(Sprites(CarpetaSprites + "/heroe_caminar.png").Length == 8 && Sprites(CarpetaSprites + "/tridente_caminar.png").Length == 8, "caminar: 8 sprites en héroe y tridente");
        var caminar = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Animaciones/Heroe/caminar.anim");
        Chk(caminar.isLooping && Mathf.Abs(caminar.length - 0.64f) < 0.015f, $"caminar en bucle, {caminar.length:0.00} s");
        var aterrizar = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Animaciones/Heroe/aterrizar.anim");
        Chk(!aterrizar.isLooping && Mathf.Abs(aterrizar.length - 0.17f) < 0.015f, $"aterrizar sin bucle, {aterrizar.length:0.00} s");
        foreach (var c in new[] { "Heroe/Heroe", "Tridente/Tridente" })
        {
            var ctrl = AssetDatabase.LoadAssetAtPath<AnimatorController>($"Assets/Animaciones/{c}.controller");
            Chk(ctrl.layers[0].stateMachine.states.Length == 6 && ctrl.layers[0].stateMachine.defaultState.name == "reposo", $"{c}.controller: 6 estados, empieza en reposo");
        }
    }
}
#endif
