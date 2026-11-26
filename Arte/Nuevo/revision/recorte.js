// Recorte de los fotogramas de las hojas del protagonista (compartido por las páginas de revisión).
const RAIZ = '../../../Assets/Sprites/';
const HOJAS = {
  tridente: RAIZ + 'Heroe/WhatsApp Image 2026-11-14 at 21.19.24.jpeg',
  espada:   RAIZ + 'Heroe/WhatsApp Image 2026-11-14 at 21.13.55.jpeg',
};
const MAPA = RAIZ + 'Mapa/WhatsApp Image 2026-11-14 at 20.25.32.jpeg';

// Zonas de cada animación en las hojas de 1600x800 (las dos tienen la misma maqueta).
const ZONAS = {
  caminar:   { r:[34, 99, 758, 99],  n:8, ms:80 },
  correr:    { r:[822, 99, 750, 99], n:8, ms:60 },
  saltar:    { r:[34, 250, 738, 136], n:7, ms:90 },
  ataque:    { r:[804, 278, 768, 108], n:6, ms:70 },
  especial:  { r:[34, 478, 768, 86],  n:6, ms:90 },
  defender:  { r:[834, 482, 348, 98], n:4, ms:110 },
  danio:     { r:[1208, 482, 364, 98], n:4, ms:110 },
  interactuar:{ r:[36, 656, 362, 92], n:4, ms:150 },
  recoger:   { r:[424, 656, 346, 92], n:4, ms:130 },
  leer:      { r:[798, 656, 370, 92], n:4, ms:180 },
  derrota:   { r:[1194, 656, 376, 92], n:4, ms:180 },
};
const NOMBRES = { caminar:'Caminar', correr:'Correr', saltar:'Saltar', ataque:'Ataque', especial:'Ataque especial', defender:'Defender', danio:'Recibir daño', interactuar:'Interactuar', recoger:'Recoger objeto', leer:'Usar objeto / Leer', derrota:'Derrota' };

const cargar = src => new Promise((ok, mal) => { const i = new Image(); i.onload = () => ok(i); i.onerror = () => mal(src); i.src = src; });

// Quita el fondo azul oscuro (relleno desde el borde de la zona) y separa los fotogramas.
// cerrar = radio en píxeles: el relleno no entra por rendijas más estrechas que el doble de ese radio.
// Sin eso, la túnica (azul marino como el fondo) se borra cuando el contorno tiene un hueco.
function recortar(img, [x0, y0, w, h], n, cerrar = 0) {
  const c = document.createElement('canvas'); c.width = w; c.height = h;
  const g = c.getContext('2d', { willReadFrequently:true }); g.drawImage(img, -x0, -y0);
  const datos = g.getImageData(0, 0, w, h), p = datos.data;
  const fondo = new Uint8Array(w * h), pila = [];
  // El fondo es azul marino (azul claramente mayor que rojo); el pelo es negro neutro y se conserva.
  const parecido = i => p[i+2] - p[i] >= 12 && p[i+2] < 75 && p[i+1] < 60;
  // Zona prohibida para el relleno: todo lo que está a "cerrar" píxeles o menos del dibujo.
  const cerca = new Uint8Array(w * h);
  if (cerrar) for (let y = 0; y < h; y++) for (let x = 0; x < w; x++) {
    if (parecido((y*w + x) * 4)) continue;
    for (let dy = -cerrar; dy <= cerrar; dy++) for (let dx = -cerrar; dx <= cerrar; dx++) {
      const xx = x + dx, yy = y + dy; if (xx >= 0 && xx < w && yy >= 0 && yy < h) cerca[yy*w + xx] = 1;
    }
  }
  const vecinos = k => { const x = k % w; return [x > 0 ? k-1 : -1, x < w-1 ? k+1 : -1, k >= w ? k-w : -1, k < w*(h-1) ? k+w : -1]; };
  for (let x = 0; x < w; x++) pila.push(x, (h-1)*w + x);
  for (let y = 0; y < h; y++) pila.push(y*w, y*w + w - 1);
  while (pila.length) {
    const k = pila.pop(); if (fondo[k] || cerca[k] || !parecido(k * 4)) continue;
    fondo[k] = 1;
    for (const v of vecinos(k)) if (v >= 0) pila.push(v);
  }
  // El fondo vuelve a crecer hasta tocar el dibujo, pero solo por píxeles de color de fondo.
  for (let paso = 0; paso < cerrar + 1; paso++) {
    const nuevo = [];
    for (let k = 0; k < w*h; k++) if (!fondo[k] && parecido(k * 4) && vecinos(k).some(v => v >= 0 && fondo[v])) nuevo.push(k);
    for (const k of nuevo) fondo[k] = 1;
  }
  for (let k = 0; k < w*h; k++) if (fondo[k]) p[k*4+3] = 0;
  g.putImageData(datos, 0, 0);

  // Cortes entre fotogramas: las n-1 columnas más vacías, separadas al menos media celda.
  const col = new Float32Array(w);
  for (let x = 0; x < w; x++) for (let y = 0; y < h; y++) if (!fondo[y*w + x]) col[x]++;
  const suave = col.map((_, x) => { let s = 0; for (let d = -3; d <= 3; d++) s += col[Math.min(w-1, Math.max(0, x+d))]; return s; });
  const celda = w / n, cortes = [];
  for (const x of [...suave.keys()].filter(x => x > celda*0.4 && x < w - celda*0.4).sort((a, b) => suave[a] - suave[b]))
    if (cortes.length < n - 1 && cortes.every(c2 => Math.abs(c2 - x) > celda * 0.55)) cortes.push(x);
  cortes.sort((a, b) => a - b);
  const bordes = [0, ...cortes, w];

  const marcos = [];
  for (let i = 0; i + 1 < bordes.length; i++) {
    const cx0 = bordes[i], cx1 = bordes[i+1];
    // Piezas conexas dentro de la celda.
    const et = new Int32Array(w * h).fill(-1), comps = [];
    for (let y = 0; y < h; y++) for (let x = cx0; x < cx1; x++) {
      const k = y*w + x; if (fondo[k] || et[k] >= 0) continue;
      const b = { x0:1e9, y0:1e9, x1:-1, y1:-1, n:0, px:[] }, id = comps.length; comps.push(b);
      const q = [k]; et[k] = id;
      while (q.length) {
        const j = q.pop(), xx = j % w, yy = (j / w) | 0; b.n++; b.px.push(j);
        if (xx < b.x0) b.x0 = xx; if (xx > b.x1) b.x1 = xx; if (yy < b.y0) b.y0 = yy; if (yy > b.y1) b.y1 = yy;
        for (const v of [xx>cx0?j-1:-1, xx<cx1-1?j+1:-1, j-w, j+w]) if (v >= 0 && v < w*h && !fondo[v] && et[v] < 0) { et[v] = id; q.push(v); }
      }
    }
    if (!comps.length) continue;
    const mayor = comps.reduce((a, b) => b.n > a.n ? b : a);
    // Se queda con el cuerpo y las piezas que lo acompañan (arma, chispas); fuera ruido, etiquetas y trozos del vecino.
    const vale = b => b === mayor || (b.n >= 25 && b.y1 - b.y0 > 4 && !(b.y1 < mayor.y0 + 2 && b.y1 - b.y0 <= 12) && b.y0 < mayor.y1 - 3 && b.x0 > cx0 + 1 && b.x1 < cx1 - 2);
    const m = { x0:1e9, y0:1e9, x1:-1, y1:-1, px:[] };
    for (const b of comps.filter(vale)) { m.x0 = Math.min(m.x0,b.x0); m.y0 = Math.min(m.y0,b.y0); m.x1 = Math.max(m.x1,b.x1); m.y1 = Math.max(m.y1,b.y1); m.px.push(...b.px); }
    if (m.y1 - m.y0 >= 30) marcos.push(m);
  }
  return marcos.map(m => {
    const fw = m.x1 - m.x0 + 1, fh = m.y1 - m.y0 + 1;
    const f = document.createElement('canvas'); f.width = fw; f.height = fh;
    const fg = f.getContext('2d'), sal = fg.createImageData(fw, fh);
    for (const j of m.px) { const o = (((j / w) | 0) - m.y0) * fw + (j % w - m.x0); sal.data.set(p.subarray(j*4, j*4+4), o*4); }
    fg.putImageData(sal, 0, 0);
    // Ancla: centro del pelo (las 20 filas de arriba de la cabeza) y los pies abajo.
    const filas = fg.getImageData(0, 0, fw, Math.min(20, fh)).data;
    let sx = 0, n = 0;
    for (let k = 0; k < filas.length / 4; k++) if (filas[k*4+3]) { sx += k % fw; n++; }
    return { lienzo:f, ax: n ? sx / n : fw / 2, alto: fh };
  });
}

