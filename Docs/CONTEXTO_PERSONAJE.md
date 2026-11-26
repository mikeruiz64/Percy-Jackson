# CONTEXTO_PERSONAJE: Alexios

> Documento de referencia del personaje. Las decisiones escritas aquí son las que definen el diseño.
> Última actualización: 2026-11-16 (el usuario pidió sustituir el diseño anterior por sus hojas de animaciones nuevas).

---

## 1. Resumen

Protagonista de "Los Herederos del Olimpo" (proyecto **Percy Jackson**, nombre provisional). Chico chibi en pixel art, hijo de un dios del mar. Se llama **Alexios**. Su arma inicial es una **daga**; el **tridente** queda para más adelante.

Referencias (del usuario, 2026-11-14):
- `Assets/Sprites/Heroe/WhatsApp Image 2026-11-14 at 21.13.55.jpeg`: hoja completa con espada (base de la versión con daga).
- `Assets/Sprites/Heroe/WhatsApp Image 2026-11-14 at 21.19.24.jpeg`: hoja completa con tridente.
- Las de las 20.25 y 20.53 son versiones anteriores más cortas de las mismas animaciones.

El diseño anterior (vista de frente, 45×70, tridente en un sprite aparte) está en `Arte/Heroe/` y en el historial de git, pero ya no se usa.

---

## 2. Diseño visual

### 2.1 Estilo y tamaño
- **Pixel art chibi de perfil**, mirando a la **derecha**. Contorno fino de 1 px en azul muy oscuro (`#1c1a2b`).
- Variante de limpieza **"B · Pixel art"** (elegida el 2026-11-16): las hojas se reducen al 72 %, se pasan a una paleta fija y se quitan los píxeles sueltos. Unos 65 px de alto.
- **Sin sombras negras:** las sombras van en tonos oscuros del propio color y el pelo en azul muy oscuro.

### 2.2 Aspecto
- Pelo negro azulado, despeinado y en punta. Ojos verdes. Piel morena clara.
- Túnica azul marino con bordes dorados, capa azul con greca dorada, cinturón con hebilla dorada y botas marrones.
- Paleta fija en `Arte/Nuevo/revision/daga.html` (`PALETA`).

### 2.3 Armas
- **Daga (arma inicial):** la espada de la hoja recortada a 14 px de hoja, con punta de acero. Envainada se ve la empuñadura a la espalda.
- **Tridente:** hojas `heroe_tridente_*` ya exportadas, para cuando se consiga.

---

## 3. Animaciones

Mismo juego de animaciones que las hojas del usuario. En el juego, saltar se divide en subir, cima, caer y aterrizar.

| Animación | Fotogramas | Duración |
|---|---|---|
| Reposo | 1 (primer fotograma de caminar) | — |
| Caminar | 8 | 80 ms |
| Correr | 8 | 60 ms |
| Saltar | 7: impulso, despegue, subida, cima, caída, aterrizaje, recuperar | subir 2×90 · cima 120 · caer 120 · aterrizar 80+90 ms |
| Ataque | 6 | 70 ms |
| Ataque especial | 6 | 90 ms |
| Defender, recibir daño | 4 | 110 ms |
| Interactuar, recoger objeto, usar objeto/leer, derrota | 4 | 150 / 130 / 180 / 180 ms |

---

## 4. Pendiente

- Reposo propio (ahora es un solo fotograma).
- Más detalle en el dibujo: con las hojas en JPEG no se puede añadir. Haría falta el original en PNG o regenerarlas.
- Escenario con el estilo de la imagen de Atenas (`Assets/Sprites/Mapa/`).

---

## 5. Decisiones cerradas (NO volver a preguntar)

- Nombre: Alexios.
- Estilo y diseño: los de las hojas del usuario del 14/11 (de perfil, capa, paleta azul y dorada). Este estilo es la referencia para **todos** los sprites nuevos, también los de otros personajes.
- Limpieza "B · Pixel art", sin sombras negras.
- Arma inicial: daga.
