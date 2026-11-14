# CONTEXTO_PERSONAJE: Héroe del tridente

> Documento de referencia del personaje. Las decisiones escritas aquí son las que definen el diseño.

---

## 1. Resumen

Protagonista del juego **Percy Jackson** (nombre provisional del proyecto). Chico de estilo chibi con temática marina, al estilo de un hijo de Poseidón, armado con un **tridente dorado**. Se llama **Alexios** (protagonista de "Los Herederos del Olimpo", decidido el 2026-11-14).

Referencia original: [Arte/Heroe/referencia/referencia_original.png](../Arte/Heroe/referencia/referencia_original.png).

---

## 2. Diseño visual

### 2.1 Estilo y tamaño
- **Pixel art** chibi, igual que la referencia (se descartó una versión dibujada a mano estilo *Hollow Knight*).
- Sprite de **45×70 píxeles**, reconstruido píxel a píxel desde la captura de referencia (que estaba ampliada ×5,78).
- Vista de tres cuartos, mirando hacia la **derecha** de la imagen (corregido el 2026-11-14: la oreja visible está a la izquierda). Contorno oscuro.

### 2.2 Aspecto
- **Pelo:** negro, despeinado y en punta, con mechones sobre la frente.
- **Piel:** morena.
- **Ojos:** verdes y brillantes, con un destello amarillo en forma de "+" y centro blanco.
- **Ropa:** túnica azul marino con bordes dorados en mangas y cuello, tridente dorado bordado en el pecho, cinturón marrón con hebilla de tridente, pantalón corto azul marino con dos botones dorados en cada pierna y botas marrones.

### 2.3 Tridente
- **Sprite aparte** del personaje, para poder mostrarlo u ocultarlo.
- Dorado, con mango de madera y cabeza en forma de "Ψ" apuntando hacia abajo a la derecha.
- Se dibuja **encima** del personaje y alineado con él (mismo lienzo de 45×70). El tramo del mango que queda detrás de la mano no está en esa capa, así que la mano se ve delante.
- Versión suelta y completa (27×27) para usarlo como objeto.

---

## 3. Animaciones

| Animación | Sprite | Comportamiento definido |
|---|---|---|
| Reposo | Personaje | La cabeza y el torso bajan 1 px al respirar; las botas no se mueven. 4 fotogramas, 1,28 s. |
| Brillo | Tridente | Quieto y, cada 2 s, un destello recorre las puntas en diagonal. 6 fotogramas, 2,03 s. |
| Caminar | Personaje + tridente | Como el caminar de Naia, adaptado a la vista de frente: la pierna que avanza sube 3 px, el cuerpo baja 1 px al recibir el peso y los brazos se mueven 2 px al revés que las piernas. 8 fotogramas de 80 ms (0,64 s, en bucle). El tridente sigue a la mano. |
| Saltar | Personaje + tridente | Como el salto de Naia: impulso (aplastado), despegue y subida (brazos 3 px arriba), cima (piernas recogidas 3 px), caída, aterrizaje (aplastado 3 px) y recuperar. En el juego: `subir` (2-3), `cima` (4), `caer` (5), `aterrizar` (6-7). |

---

## 4. Pendiente

- Resto de animaciones (ataque con el tridente, poderes de agua…): sin definir.

---

## 5. Decisiones cerradas (NO volver a preguntar)

- Pixel art como la referencia, 45×70.
- Diseño del personaje tal como está en la referencia.
- Tridente como sprite aparte, encima del personaje.
- Reposo (cabeza y torso) y brillo del tridente.
- Nombre: Alexios.
- Caminar "C · Paso rápido" y saltar "B · Salto amplio" (2026-11-14), con la estructura de las animaciones de Naia.
