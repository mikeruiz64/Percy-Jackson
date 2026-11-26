# CONTEXTO

> Última actualización: 2026-11-16

## Objetivo

Videojuego 2D en **Unity 6** (nombre provisional: **Percy Jackson**). El protagonista es **Alexios**, un chico chibi en pixel art de perfil, con daga como arma inicial.

El diseño del personaje está en [Docs/CONTEXTO_PERSONAJE.md](Docs/CONTEXTO_PERSONAJE.md), que es la **fuente de verdad** del personaje.

## Historia del proyecto

1. El usuario aportó una captura de su personaje en pixel art. Se reconstruyó el sprite original (45×70) a partir de la captura.
2. Se aprobó la animación de reposo y se separó el tridente en un sprite propio, con su animación de brillo.
3. Se probó una versión dibujada a mano estilo *Hollow Knight*, que se descartó: el personaje se queda en pixel art.
4. El 2026-10-15 se creó este proyecto de Unity y se trasladaron aquí todos los archivos del personaje.
5. El 2026-11-16 el usuario sustituyó el diseño por sus hojas de animaciones nuevas (de perfil, con capa). Se recortan y limpian con un script (variante "B · Pixel art") y la daga pasa a ser el arma inicial.

Es un juego **distinto** del de Gota (proyecto de Godot "Juego Gota").

Desde el 2026-11-14 es la implementación del proyecto de la materia **Desarrollo de Videojuegos (UTS)**, "Los Herederos del Olimpo": el héroe es **Alexios**. Documentos de la materia en `OneDrive\Documentos\UTS\4to Semestre\Desarrollo de Videojuegos\`.

## Decisiones

- **Motor:** Unity 6000.4.8f1 con URP 2D, misma configuración base que el proyecto CazadorMonedas del usuario.
- **Pixel art nítido:** filtro *Point*, sin compresión ni mipmaps, 32 píxeles por unidad.
- **Pivote en los pies** (abajo en el centro). Todos los fotogramas comparten celda, con la cabeza centrada.
- **El arma va dibujada en el sprite** (daga). Ya no hay tridente aparte ni componente `Heroe`.
- **Construcción por código:** `Assets/Editor/ConstruirPercyJackson.cs` crea los sprites, clips, controladores, el prefab y la escena a partir de las hojas exportadas. No se editan a mano.
- **Arte fuera de `Assets/`:** las fuentes (`.aseprite`, scripts y página de revisión) están en `Arte/`, que Unity no importa.
- **Prólogo:** recorrido A "Calle hacia la colina" y estilo "Amanecer dorado" (elegidos el 2026-11-14). El nivel es una imagen de fondo a 32 px/unidad y los colliders son invisibles; las medidas están en `mapa.js` y en `ConstruirPrologo.cs` y deben coincidir.
- **Animaciones (2026-11-16):** 15 estados en `Heroe.controller` (reposo, caminar, correr, subir, cima, caer, aterrizar, ataque, especial, defender, danio, interactuar, recoger, leer, derrota). `Alexios.cs` elige el estado con `Animator.Play`, sin transiciones.
- **Hojas del protagonista:** `Arte/Nuevo/scripts/exportar_unity.py` abre `Arte/Nuevo/revision/daga.html?exportar` en Brave sin ventana. Esa página recorta y limpia las hojas JPEG del usuario y escribe `heroe_*` (daga) y `heroe_tridente_*` en `Assets/Sprites/Heroe/`.
- **Nombre:** el juego se llama "Los Herederos del Olimpo" (`productName`). La carpeta y el repositorio siguen como "Percy Jackson".
- **Estados del juego (2026-11-16):** `Juego.cs` se crea solo al arrancar y guarda el estado (Menu, Jugando, Dialogo, Codice, Pausa, Recompensa). Solo en Jugando corre el tiempo y Alexios obedece.
- **Entradas:** todo pasa por `Controles.cs` (teclado y mando).
- **Guardado:** `Guardado.cs` escribe `partida.json` en la carpeta de datos del usuario; se guarda en los `PuntoControl`. Los pergaminos recogidos después del último punto no se guardan.
- **Interfaz:** `Interfaz.cs` crea por código el lienzo (uGUI): marcador, mensajes, menú principal y pausa, en azul marino y dorado.
- **Diseño de la fase:** `Arte/Diseno/revision/index.html` (Prólogo + Capítulo 1, aprobado el 2026-11-16).
- **Revisión en el navegador:** cada animación nueva se enseña en una página local con varias propuestas antes de darla por buena.

## Arquitectura

```
Percy Jackson/
├── Assets/
│   ├── Sprites/Heroe/        Hojas del usuario (JPEG) y hojas limpias heroe_* / heroe_tridente_* (PNG + JSON)
│   ├── Animaciones/Heroe/    15 clips y Heroe.controller (generados)
│   ├── Prefabs/Heroe.prefab  Alexios con daga (generado)
│   ├── Escenas/Principal.unity   Escena de prueba (generada)
│   ├── Escenas/Prologo.unity     Prólogo: recorrido "Calle hacia la colina", recoger pergaminos (generada)
│   ├── Scripts/Alexios.cs    Caminar, correr, saltar, atacar y mecánica de pergaminos
│   ├── Scripts/Juego.cs · Controles.cs · Guardado.cs · Interfaz.cs · PuntoControl.cs   Base del juego
│   ├── Scripts/CamaraSigue.cs   La cámara sigue a Alexios en X, con límites
│   ├── Sprites/Mapa/         Arte del Prólogo exportado (nivel, pergamino, santuario, mapa.json)
│   ├── Fisica/SinFriccion    Material de física de Alexios
│   ├── Editor/ConstruirPercyJackson.cs   Construye todo lo anterior
│   ├── Editor/ConstruirPrologo.cs        Construye la escena del Prólogo
│   └── Settings/ …           Configuración de URP 2D
├── Docs/CONTEXTO_PERSONAJE.md
├── Arte/Nuevo/              Diseño actual: revision/ (index.html vista previa jugable, daga.html propuestas y exportación, recorte.js) · scripts/exportar_unity.py
└── Arte/Heroe/               Diseño anterior (de frente, sin uso)
    ├── heroe.aseprite        Personaje sin tridente (etiqueta reposo)
    ├── tridente/             tridente.aseprite (brillo), tridente_en_mano.png, tridente.png
    ├── heroe_base*.png       Pose base con y sin tridente
    ├── scripts/              heroe_sprites.lua · heroe_movimiento.lua · heroe_final.lua · exportar_unity.py · reconstruir_desde_captura.lua
    ├── revision/             Página de revisión (index.html + servidor.py)
    └── referencia/           Captura original del personaje
Arte/Mapa/                    mapa.js (dibujo del nivel) · exportar.html + exportar_mapa.py · revision/ (recorridos y estilos)
```

## Entorno

Windows 11, Unity 6000.4.8f1, Aseprite compilado desde el código fuente e instalado en `C:\Herramientas\Aseprite\aseprite.exe` y Python 3.14. Equipo con 8 GB de RAM y gráfica integrada.
