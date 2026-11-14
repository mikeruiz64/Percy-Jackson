# AUDITORÍA

> Registro de revisiones, pruebas y decisiones. Entrada nueva arriba.

## Plantilla

```
## AUD-XXX — AAAA-MM-DD — Título
**Alcance:** · **Hallazgos:** · **Riesgos:** · **Acciones:** · **Estado:**
```

---

## AUD-004 — 2026-11-14 — Caminar y saltar como Naia

**Alcance:** animaciones de movimiento de Alexios tomando como modelo las de Naia (`Juego Naia/04_Arte/Sprites_Oficiales/Naia/agua`).

**Decisiones:** misma estructura que Naia (caminar 8 fotogramas; saltar 7, partido en subir/cima/caer/aterrizar), adaptada a la vista de frente: levantar la pierna en vez de adelantarla. `heroe_movimiento.lua` mueve por separado piernas, cuerpo, brazos y tridente (el tridente sigue a la mano izquierda). Elegidos "C · Paso rápido" (3 px, 80 ms) y "B · Salto amplio" (brazos 3 px, piernas 3 px). Se quitó el desplazamiento lateral de los brazos en la cima porque dejaba una raya transparente. Animator sin transiciones: `Alexios.ElegirAnimacion` llama a `Play` en los dos Animator solo cuando cambia el estado.

**Incidente:** al guardar el manifiesto, lo leído con `json.decode` (userdata) dentro de una tabla nueva se guardó como null y se perdieron el reposo y el brillo; `heroe_final.lua` (aún la versión antigua) dejó los `.aseprite` vacíos. Recuperado: manifiesto restaurado con su contenido original, función `plano()` en el script y reconstrucción completa. `heroe_final.lua` ahora falla si no encuentra fotogramas.

**Pruebas:** exportación: reposo 4 fot. (360/280/360/280), caminar 8×80 ms, aterrizar 80+90 ms. Constructor del héroe 11/11 [OK] (margen de 15 ms por el redondeo de Unity) y del Prólogo 8/8 [OK]. `.exe` con teclas simuladas: camina, salta, aterriza en el tejado y se gira.

**Riesgos y límites:** no se ha jugado entero. Mientras camina en el aire de un borde a otro sin saltar se ve `caer` (correcto). Los brazos solo suben y bajan (vista de frente).

**Estado:** abierta hasta que el usuario lo juegue.

## AUD-003 — 2026-11-14 — Giro, recorrido y arte del Prólogo

**Alcance:** corregir el giro de Alexios, nuevo recorrido y estilo visual.

**Hallazgos:** el sprite original mira a la **derecha** (el documento del personaje decía izquierda), así que se giraba al revés. Sin material de física, la fricción lo dejaba pegado a las paredes.

**Decisiones:** escala X = 1 a la derecha y −1 a la izquierda; material `SinFriccion` (fricción 0). Recorrido A "Calle hacia la colina" (x −9 a 33) con `CamaraSigue` (LateUpdate, límites −0,3 y 24,1). Estilo "Amanecer dorado" elegido entre tres en `Arte/Mapa/revision/visual.html`. El arte sale de `Arte/Mapa/mapa.js` y se exporta a PNG con Brave sin ventana (`exportar_mapa.py`); pivotes calculados para que pergamino y santuario encajen con su collider. Las partes sólidas son colliders invisibles.

**Pruebas:** constructor 8/8 [OK]; compilación de Windows correcta; captura de la ventana del juego igual que la propuesta.

**Riesgos y límites:** no se ha jugado entero (saltos calculados, no probados). Las medidas del recorrido están dos veces (`mapa.js` y `ConstruirPrologo.cs`): si se cambia una, hay que cambiar la otra. Sin paralaje ni animación de caminar.

**Estado:** abierta hasta que el usuario lo juegue.

## AUD-002 — 2026-11-14 — Prólogo: mecánica de recoger pergaminos

**Alcance:** taller de la UTS (escena + mecánica). El héroe pasa a llamarse Alexios.

**Decisiones:** vista lateral con salto; teclado con Input Manager como en la guía de clase; 3 pergaminos (trigger + etiqueta) y santuario que se abre con la meta; salto solo en suelo (`OnCollisionStay2D`, normal.y > 0,5); salto variable y gravedad 3 como ajuste de mejora; sonidos WAV generados por el constructor; objetos con un cuadrado blanco coloreado.

**Pruebas:** Unity 6000.4.8f1 en consola: compila; `ConstruirPrologo` da 5/5 [OK] (pergaminos y meta, santuario, sonidos, física de Alexios, escena guardada).

**Riesgos y límites:** no se ha jugado con ventana; alturas de salto calculadas (máx. ≈ 3,3 u; plataformas a 2,7 u), no comprobadas. Sin animación de caminar (mientras camina se ve el reposo). `ConstruirPercyJackson` deja en Build Settings solo `Principal`; volver a pasar `ConstruirPrologo` después.

**Estado:** abierta hasta la prueba del usuario.

## AUD-001 — 2026-10-15 — Creación del proyecto con el héroe del tridente

**Alcance:** creación del proyecto de Unity, traslado del arte del héroe y primeras animaciones en el juego.

**Decisiones:**
- Sprite reconstruido desde la captura de referencia: cuadrícula detectada con escala ×5,78, color de cada celda por votación de su zona central y paleta agrupada. Resultado: 45×70 px. Se redibujó el destello en "+" de los ojos, que se perdía al reducir.
- Reposo aprobado: cabeza y torso bajan 1 px (variante "B").
- Tridente separado en su propio sprite. La pierna y la bota izquierdas, que tapaba, se reconstruyeron reflejando las derechas. Se quitaron los destellos blancos sueltos de la referencia.
- Versión dibujada a mano estilo *Hollow Knight* descartada por el usuario.
- Configuración base de Unity copiada del proyecto CazadorMonedas (URP 2D, paquetes y ajustes), con nombre y `productGUID` propios.
- Escena, prefab, clips y controladores generados por `ConstruirPercyJackson.cs`. Clips a 100 fps internos para respetar las duraciones de Aseprite (múltiplos de 10 ms).

**Pruebas:**
- Unity 6000.4.8f1 en modo consola: el proyecto importa y compila sin errores.
- Comprobaciones del constructor: `heroe_reposo` 4 sprites, `tridente_brillo` 6 sprites, ambos clips en bucle (1,29 s y 2,04 s; 10 ms de redondeo de Unity), prefab con el tridente como hijo y escena guardada.
- La combinación personaje + tridente se comprobó visualmente en la página de revisión: se ve igual que el sprite original.

**Riesgos y límites:**
- La escena no se ha abierto en el editor con ventana: falta comprobar a simple vista el encaje del tridente y la escala en la vista de juego.
- La pierna y la bota izquierdas reconstruidas son una suposición (no se veían en la referencia).

**Estado:** abierta hasta la prueba del usuario en el editor.
