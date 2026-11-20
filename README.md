# Percy Jackson: Los Herederos del Olimpo

Videojuego 2D de plataformas en **Unity 6** con pixel art. El héroe es **Alexios**, un chico de temática marina que lleva un tridente.

> Proyecto de la materia Desarrollo de Videojuegos (UTS). En desarrollo.

## Qué hay hecho

- **Prólogo jugable "Calle hacia la colina":** Alexios camina, salta y recoge pergaminos. Con todos los pergaminos, el santuario se ilumina y al entrar se gana el nivel.
- **Salto variable:** un toque corto da un salto bajo y mantener la tecla da un salto alto.
- **Animaciones** de reposo, caminar, subir, cima, caer y aterrizar, sincronizadas entre el héroe y el tridente (que es un objeto hijo con su propio Animator).
- **Cámara que sigue** al personaje y material de física sin fricción.
- **Construcción por código:** scripts de editor que crean sprites, clips, prefab y escenas a partir de las hojas exportadas.

## Controles

| Tecla | Acción |
|---|---|
| ← → o A D | Caminar |
| Espacio | Saltar (mantener para saltar más alto) |
| R | Reiniciar después de ganar |

## Cómo probarlo

1. Abre Unity Hub → **Add** → elige esta carpeta. Usa Unity **6000.4.8f1**.
2. Abre `Assets/Escenas/Prologo.unity` y pulsa **Play**.

## Estructura

| Ruta | Contenido |
|---|---|
| `Assets/Scripts/Alexios.cs` | Movimiento, salto, animaciones y mecánica de pergaminos |
| `Assets/Scripts/CamaraSigue.cs` | Cámara que sigue al héroe |
| `Assets/Scripts/Heroe.cs` | Mostrar u ocultar el tridente |
| `Assets/Editor/` | Scripts que construyen el prefab y las escenas |
| `Assets/Escenas/` | Escena principal y Prólogo |
| `Arte/` | Fuentes del arte y del mapa (Unity no las importa) |

## Tecnologías

Unity 6 (URP 2D) · C# · Aseprite · JavaScript y Python (diseño y exportación del mapa)

## Autor

Michael Ruiz · [LinkedIn](https://www.linkedin.com/in/michael-ruiz-04244b370) · [GitHub](https://github.com/mikeruiz64)
