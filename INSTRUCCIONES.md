# INSTRUCCIONES

> Última actualización: 2026-11-16

## Abrir y probar el juego

1. Abre Unity Hub → **Add** → elige esta carpeta. Usa Unity **6000.4.8f1**.
2. Abre `Assets/Escenas/Principal.unity` y pulsa **Play**: se ve a Alexios en reposo.
3. **Prólogo (taller):** abre `Assets/Escenas/Prologo.unity` y pulsa Play. Empieza en el menú (↑ ↓ y Enter). ← → / A D caminar, Shift correr, Espacio saltar, J atacar con la daga, Esc pausa, R reiniciar tras ganar. La partida se guarda al pasar por los puntos de control (`%USERPROFILE%\AppData\LocalLow\DefaultCompany\Los Herederos del Olimpo\partida.json`). Se reconstruye con **Herramientas → Construir Prólogo** (después de *Construir Percy Jackson*, que hace el prefab).
   - Para cambiar el arte o el recorrido: editar `Arte/Mapa/mapa.js` (y las mismas medidas en `ConstruirPrologo.cs`), revisar en `Arte/Mapa/revision/visual.html`, exportar con `python -I Arte\Mapa\exportar_mapa.py` y volver a construir.

## Usar al héroe en otras escenas

- Arrastra `Assets/Prefabs/Heroe.prefab` a la escena. Su pivote está en los pies.
- Para mirar al otro lado: escala X negativa en el objeto **Heroe**.

## Animaciones del protagonista (diseño actual)

1. **Ver y comparar:** `python -m http.server 8765 --bind 127.0.0.1` en la carpeta del proyecto y abrir en Brave http://127.0.0.1:8765/Arte/Nuevo/revision/index.html (vista previa jugable) o `.../daga.html` (propuestas de limpieza y daga).
2. **Exportar al juego:** `python -I Arte\Nuevo\scripts\exportar_unity.py` (crea `heroe_*` y `heroe_tridente_*` en `Assets/Sprites/Heroe/`).
3. **Construir:** *Herramientas → Construir Percy Jackson* y después *Construir Prólogo*.

## Diseño anterior (de frente, sin uso)

Requisitos: el Aseprite compilado (`C:\Herramientas\Aseprite\aseprite.exe`) y Python 3. Las rutas son relativas a la carpeta del proyecto.

1. **Generar propuestas** (sprites del personaje y del tridente a partir de `Arte/Heroe/heroe_base_con_tridente.png`):
   ```
   aseprite -b --script-param dir="Arte\Heroe" --script Arte\Heroe\scripts\heroe_sprites.lua
   ```
2. **Revisar** en el navegador:
   ```
   python -I Arte\Heroe\revision\servidor.py 8766
   ```
   http://127.0.0.1:8766 — se puede votar, elegir favorita y comentar (se guarda en `Arte/Heroe/revision/opiniones.json`).
   Caminar y saltar (como Naia) se generan aparte, sin tocar las animaciones ya aprobadas:
   ```
   aseprite -b --script-param dir="Arte\Heroe" --script Arte\Heroe\scripts\heroe_movimiento.lua
   ```
3. **Montar en Aseprite** las animaciones aprobadas (`heroe.aseprite` y `tridente/tridente.aseprite`):
   ```
   aseprite -b --script-param dir="Arte\Heroe" --script Arte\Heroe\scripts\heroe_final.lua
   ```
4. **Exportar al juego** (hojas en `Assets/Sprites/Heroe/`):
   ```
   python -I Arte\Heroe\scripts\exportar_unity.py
   ```
5. En Unity: **Herramientas → Construir Percy Jackson** (o en consola: `Unity -batchmode -projectPath . -executeMethod ConstruirPercyJackson.Construir -quit`). Rehace sprites, clips, controladores, prefab y escena.

El diseño del personaje manda: cualquier animación debe respetar [Docs/CONTEXTO_PERSONAJE.md](Docs/CONTEXTO_PERSONAJE.md), y sus decisiones cerradas no se cambian sin que lo pida el usuario.

## Git y GitHub

- Mensajes de commit **en español**, cortos y descriptivos.
- Los commits los firma el usuario. **No se añaden coautores, firmas ni menciones a herramientas** en commits, ramas, pull requests ni archivos del proyecto.
- No se suben: `Library/`, `Temp/`, `Logs/`, `UserSettings/`, `Arte/*/revision/sprites/` ni `opiniones.json`.

## Flujo de cada sesión

1. Leer [PENDIENTES.md](PENDIENTES.md) y [CONTEXTO.md](CONTEXTO.md).
2. Proponer las animaciones o cambios en la página de revisión y esperar la aprobación del usuario.
3. Montar, exportar, construir en Unity y comprobar que la escena funciona (el constructor escribe `[OK]`/`[FALLO]` en la consola).
4. Actualizar PENDIENTES, CONTEXTO si cambia algo importante, y añadir una entrada en [AUDITORIA.md](AUDITORIA.md).
5. Commit y subida a GitHub.
