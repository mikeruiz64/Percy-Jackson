// Dibujo del Prólogo (recorrido A). Lo usan revision/visual.html y exportar.html.
// Las medidas deben coincidir con Assets/Editor/ConstruirPrologo.cs.
const U = 32, X0 = -9.4, X1 = 33.4, Y0 = -5, Y1 = 7;
const W = Math.round((X1 - X0) * U), H = Math.round((Y1 - Y0) * U);
const px = x => Math.round((x - X0) * U), py = y => Math.round((Y1 - y) * U);

// Recorrido A (igual que en Unity): esquina inferior izquierda, ancho, alto
const PIEDRA = [[-9,-5,19,1],[10,-5,8,2.5],[18,-5,6,4],[24,-5,9,5.5],[-9.4,-5,0.4,12],[33,-5,0.4,12]];
const CASA = [1,-4,4,2.5], ANFORA = [7,-4,0.8,1.2], COLUMNAS = [[12.5,-2.5,0.6,2.3],[15.5,-2.5,0.6,2.3]], DINTEL = [12.2,-0.2,4.2,0.4];
const PERG = [[2.8,-1.3],[14.2,0.4],[21,1.4]], SANT = [28.5,0.5,1.8,2.8];

const ESTILOS = [
  { id:'A', nombre:'Amanecer dorado', rec:true,
    texto:'Calle de Atenas al amanecer, como dice el guion. Piedra color arena, casas encaladas con tejas de terracota, columnas de mármol y el Partenón en la colina del fondo. Usa la paleta del guion: azul, arena y dorado.',
    cielo:['#2b3a67','#4b4f86','#8c6a95','#d98a7f','#f5b77f','#ffd9a0'], lejos:'#9a6b7f', lejos2:'#c48a86',
    piedra:{base:'#c9a97c', luz:'#e6cda2', sombra:'#8f7353', borde:'#f2ddb5'},
    casa:{muro:'#f1e6cc', sombra:'#d6c6a3', teja:'#c0583a', teja2:'#9c4129', puerta:'#5a3b2a'},
    marmol:{base:'#efe9dd', estria:'#cdbfa8', sombra:'#b3a48b'}, anfora:['#c0583a','#2a1c16'], fondo:'partenon' },
  { id:'B', nombre:'Mármol y mar Egeo',
    texto:'Mediodía luminoso junto al mar. Todo en mármol blanco con detalles azules y el Egeo con islas al fondo. Muy limpio y fácil de leer en pantallas de móvil.',
    cielo:['#3d8fd1','#5aa8e2','#7fc0ec','#a9d8f4','#d3eefa'], lejos:'#7ea6bd', lejos2:'#a7c4d3', mar:['#2c6f9e','#3a86b8','#5aa3cf'],
    piedra:{base:'#e3ded3', luz:'#fbf8f1', sombra:'#aea79a', borde:'#ffffff'},
    casa:{muro:'#faf7f0', sombra:'#ddd6c8', teja:'#2f6fa8', teja2:'#22547f', puerta:'#2f6fa8'},
    marmol:{base:'#f7f4ee', estria:'#d6d0c4', sombra:'#bdb5a6'}, anfora:['#c46a43','#1d1a18'], fondo:'mar' },
  { id:'C', nombre:'Niebla del olvido',
    texto:'La niebla que borra los recuerdos se ve en pantalla: colores apagados, capas de bruma y los pergaminos brillando como lo único que conserva el color. Más misterioso y ligado a la historia.',
    cielo:['#2e2a40','#463f5c','#655a78','#8b7f96','#b3a8b6'], lejos:'#5f566f', lejos2:'#7d7389',
    piedra:{base:'#8f8896', luz:'#ada6b3', sombra:'#615b6a', borde:'#c3bdc8'},
    casa:{muro:'#bab2bd', sombra:'#978f9c', teja:'#7c5967', teja2:'#5f4350', puerta:'#3e3547'},
    marmol:{base:'#c9c3cc', estria:'#a39ca8', sombra:'#857d8b'}, anfora:['#7c5967','#2a2430'], fondo:'niebla', brillo:true },
];

// opc: { heroe, pergaminos, santuario } = qué capas se dibujan
function dibujar(c, e, heroe, opc = { pergaminos: true, santuario: true }) {
  const capa = opc.capa || "todo"; // "todo", "pergamino" o "santuario" (solo esa pieza, fondo transparente)
  const solo = capa !== "todo";
  const g = c.getContext('2d');
  const R = (x, y, w, h, col) => { g.fillStyle = col; g.fillRect(x, y, w, h); };
  const RW = (x, y, w, h, col) => R(px(x), py(y + h), Math.round(w * U), Math.round(h * U), col);

  if (!solo) {
  // Cielo en bandas con tramado entre bandas (aspecto pixel art)
  const n = e.cielo.length, bh = Math.ceil(H * 0.8 / n);
  e.cielo.forEach((col, i) => R(0, i * bh, W, bh + 1, col));
  R(0, n * bh, W, H, e.cielo[n - 1]);
  for (let i = 1; i < n; i++) for (let y = i * bh - 4; y < i * bh; y += 2) for (let x = (y / 2) % 2 * 2; x < W; x += 4) R(x, y, 2, 2, e.cielo[i]);

  // Fondo lejano
  if (e.fondo === 'mar') {
    R(px(X0) + W * 0 + 900, py(4.5) - 30, 30, 30, '#fff6d8'); // sol
    const hz = py(-1.2);
    e.mar.forEach((col, i) => R(0, hz + i * 18, W, H, col));
    for (let x = 0; x < W; x += 24) R(x + (x % 48 ? 6 : 0), hz + 10, 8, 2, '#cfe8f5');
    [[60, 220, 26], [520, 300, 40], [1080, 260, 30]].forEach(([x, w, h]) => {
      g.fillStyle = e.lejos; g.beginPath(); g.moveTo(x, hz); g.quadraticCurveTo(x + w / 2, hz - h * 2, x + w, hz); g.fill(); });
  } else {
    // Colina con el Partenón y cipreses
    g.fillStyle = e.lejos2; g.beginPath(); g.moveTo(0, py(-1)); for (let x = 0; x <= W; x += 40) g.lineTo(x, py(-1) - 18 * Math.sin(x / 130) - 10); g.lineTo(W, H); g.lineTo(0, H); g.fill();
    g.fillStyle = e.lejos; g.beginPath(); g.moveTo(640, H); g.lineTo(820, py(2.2)); g.lineTo(1120, py(2.4)); g.lineTo(1300, H); g.fill();
    const tx = 900, ty = py(2.3);
    R(tx - 6, ty - 8, 170, 8, e.lejos); for (let i = 0; i < 8; i++) R(tx + i * 20, ty - 46, 8, 38, e.lejos);
    R(tx - 8, ty - 54, 174, 8, e.lejos); g.fillStyle = e.lejos; g.beginPath(); g.moveTo(tx - 8, ty - 54); g.lineTo(tx + 79, ty - 76); g.lineTo(tx + 166, ty - 54); g.fill();
    [80, 150, 420, 470, 610, 1240].forEach(x => { g.fillStyle = e.lejos; g.beginPath(); g.ellipse(x, py(-1.6), 9, 40, 0, 0, 7); g.fill(); });
    // Casas lejanas
    [[-8, 2.6, 1.6], [-2, 3, 1.2], [5.5, 2.2, 1.4], [9, 3.4, 1.8]].forEach(([x, w, h]) => RW(x, -1.4, w, h, e.lejos2));
  }

  // Piedra: sillares con junta y luz arriba
  const sillares = (x, y, w, h) => {
    const l = px(x), t = py(y + h), r = px(x + w), b = py(y);
    R(l, t, r - l, b - t, e.piedra.base);
    for (let fy = t, fila = 0; fy < b; fy += 16, fila++) {
      R(l, fy, r - l, 2, e.piedra.luz);
      R(l, fy + 14, r - l, 2, e.piedra.sombra);
      for (let fx = l + (fila % 2) * 16; fx < r; fx += 32) R(fx, fy, 2, 16, e.piedra.sombra);
    }
    R(l, t, r - l, 4, e.piedra.borde);
  };
  PIEDRA.forEach(p => sillares(...p));

  // Casa encalada con tejado
  { const [x, y, w, h] = CASA, l = px(x), t = py(y + h), r = px(x + w), b = py(y);
    R(l, t, r - l, b - t, e.casa.muro); R(r - 8, t, 8, b - t, e.casa.sombra);
    for (let i = 0; i < 40; i++) R(l + (i * 37) % (r - l - 4), t + 14 + (i * 23) % (b - t - 18), 2, 2, e.casa.sombra);
    R(l + 18, b - 44, 22, 44, e.casa.puerta); R(l + 70, t + 22, 18, 16, e.casa.puerta); R(l + 100, t + 22, 18, 16, e.casa.puerta);
    R(l - 6, t, r - l + 12, 10, e.casa.teja); for (let xx = l - 6; xx < r + 6; xx += 6) R(xx, t, 2, 10, e.casa.teja2); R(l - 6, t + 10, r - l + 12, 2, e.casa.teja2); }

  // Columnas dóricas y dintel
  const columna = ([x, y, w, h]) => { const l = px(x), t = py(y + h), r = px(x + w), b = py(y);
    R(l, t, r - l, b - t, e.marmol.base); for (let xx = l + 3; xx < r - 1; xx += 5) R(xx, t, 1, b - t, e.marmol.estria);
    R(r - 3, t, 3, b - t, e.marmol.sombra); R(l - 4, t, r - l + 8, 6, e.marmol.base); R(l - 4, t + 6, r - l + 8, 1, e.marmol.sombra); R(l - 3, b - 5, r - l + 6, 5, e.marmol.base); };
  COLUMNAS.forEach(columna);
  { const [x, y, w, h] = DINTEL, l = px(x), t = py(y + h), r = px(x + w), b = py(y);
    R(l, t, r - l, b - t, e.marmol.base); R(l, b - 2, r - l, 2, e.marmol.sombra);
    for (let xx = l + 6; xx < r - 6; xx += 16) { R(xx, t + 3, 2, b - t - 6, e.marmol.estria); R(xx + 4, t + 3, 2, b - t - 6, e.marmol.estria); } }

  // Ánfora con banda de figuras negras
  { const [x, y, w, h] = ANFORA, cx = px(x + w / 2), b = py(y), t = py(y + h);
    g.fillStyle = e.anfora[0]; g.beginPath(); g.ellipse(cx, b - 16, 12, 16, 0, 0, 7); g.fill();
    R(cx - 6, t + 2, 12, b - t - 28, e.anfora[0]); R(cx - 9, t, 18, 4, e.anfora[0]); R(cx - 12, b - 22, 24, 5, e.anfora[1]);
    R(cx - 13, t + 8, 3, 10, e.anfora[1]); R(cx + 10, t + 8, 3, 10, e.anfora[1]); }

  }
  // Santuario: frontón, columnas y puerta
  if (capa === 'santuario' || (!solo && opc.santuario)) { const [x, y, w, h] = SANT, l = px(x), r = px(x + w), b = py(y), t = py(y + h);
    R(l, t + 18, r - l, b - t - 18, e.marmol.base); R(l + 14, t + 34, r - l - 28, b - t - 34, '#3a2c2c');
    [l + 2, r - 10].forEach(cx => { R(cx, t + 18, 8, b - t - 18, e.marmol.base); R(cx + 2, t + 18, 1, b - t - 18, e.marmol.estria); R(cx + 5, t + 18, 1, b - t - 18, e.marmol.estria); });
    g.fillStyle = e.marmol.base; g.beginPath(); g.moveTo(l - 6, t + 20); g.lineTo((l + r) / 2, t - 4); g.lineTo(r + 6, t + 20); g.fill();
    R(l - 6, t + 18, r - l + 12, 3, e.marmol.sombra); R((l + r) / 2 - 3, t + 6, 6, 6, '#d9a93c'); }

  // Niebla por capas
  if (!solo && e.fondo === 'niebla') {
    [[py(-2.5), 40, .28], [py(0.5), 26, .18], [py(-4.2), 30, .32]].forEach(([y, alto, a]) => {
      g.fillStyle = `rgba(230,225,240,${a})`;
      for (let x = -40; x < W; x += 80) { g.beginPath(); g.ellipse(x + (y % 50), y, 70, alto / 2, 0, 0, 7); g.fill(); } });
  }

  // Pergaminos (enrollados, con cinta roja)
  if (capa === 'pergamino' || (!solo && opc.pergaminos)) PERG.forEach(([x, y]) => { const l = px(x), t = py(y + 0.55);
    if (e.brillo) { g.fillStyle = 'rgba(255,210,90,.35)'; g.beginPath(); g.arc(l + 6, t + 9, 18, 0, 7); g.fill(); }
    R(l, t + 2, 13, 14, '#f3e3b8'); R(l - 1, t, 15, 3, '#c9a76a'); R(l - 1, t + 15, 15, 3, '#c9a76a'); R(l, t + 8, 13, 2, '#b2392f'); R(l + 2, t + 4, 9, 1, '#c9b07e'); R(l + 2, t + 12, 7, 1, '#c9b07e'); });

  // Alexios (sprite real, 1 píxel = 1 píxel del juego)
  if (!solo && heroe && heroe.complete && heroe.naturalWidth) g.drawImage(heroe, px(-6.6) - 22, py(-4) - heroe.naturalHeight);
}
