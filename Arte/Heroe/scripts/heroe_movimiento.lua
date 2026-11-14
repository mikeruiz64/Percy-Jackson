-- Alexios: propuestas de CAMINAR y SALTAR (pixel art 45x70), con la misma estructura
-- que las animaciones de Naia (Juego Naia/04_Arte/Sprites_Oficiales/Naia/agua):
--   caminar: 8 fotogramas — contacto, bajar, paso, subir; y lo mismo con la otra pierna.
--            Siempre un pie en el suelo; los brazos van al revés que las piernas.
--   saltar:  7 fotogramas — impulso (aplastado), despegue, subida, cima (piernas recogidas),
--            caída, aterrizaje (muy aplastado), recuperar.
-- El sprite de Alexios está de frente (3/4), no de perfil como Naia: "pierna delante"
-- se traduce en levantar esa pierna, y el balanceo de brazos en subir y bajar cada brazo.
-- El tridente es una capa aparte (decisión cerrada): se mueve con la mano izquierda.
--
-- Salida: revision/sprites/<id>/NN.png (personaje + tridente, para revisar),
--         revision/sprites/<id>/heroe/NN.png y /tridente/NN.png (capas para el juego)
--         y las añade a revision/manifest.json (sin tocar las animaciones ya aprobadas).
-- Uso: aseprite -b --script-param dir=<carpeta Heroe> --script heroe_movimiento.lua

local dir = app.params.dir
local heroe = Image{ fromFile = app.fs.joinPath(dir, "heroe_base.png") }
local tridente = Image{ fromFile = app.fs.joinPath(dir, "tridente", "tridente_en_mano.png") }
local W, H = heroe.width, heroe.height
local VACIO = app.pixelColor.rgba(0, 0, 0, 0)

-- Zonas del sprite (ver el volcado de heroe_base.png)
local function brazoIzq(x, y) return x <= 16 and y >= 37 and y <= 54 end      -- el del tridente
local function brazoDer(x, y) return x >= 33 and y >= 37 and y <= 53 end
local function piernaIzq(x, y) return x <= 24 and y >= 56 end
local function piernaDer(x, y) return x >= 25 and y >= 56 end
local function cuerpo(x, y) return y <= 55 and not brazoIzq(x, y) and not brazoDer(x, y) end

-- Copia los píxeles de "img" que cumplen "zona" en "dest", desplazados (dx, dy)
local function pegar(dest, img, zona, dx, dy)
  for y = 0, H - 1 do for x = 0, W - 1 do
    if zona(x, y) then
      local p = img:getPixel(x, y)
      local nx, ny = x + dx, y + dy
      if p ~= VACIO and nx >= 0 and ny >= 0 and nx < W and ny < H then dest:drawPixel(nx, ny, p) end
    end
  end end
end

-- Una pose: cuánto sube cada pierna (px), cuánto baja el cuerpo, y el desplazamiento de cada brazo.
-- El orden de pegado hace de capas: piernas, cuerpo, brazos.
local function pose(p)
  local img = Image(W, H, ColorMode.RGB)
  local by = p.cuerpo or 0
  local sep = p.separar or 0                                   -- piernas abiertas al aplastarse
  pegar(img, heroe, piernaIzq, -sep, -(p.izq or 0))
  pegar(img, heroe, piernaDer, sep, -(p.der or 0))
  pegar(img, heroe, cuerpo, 0, by)
  local bi, bd = p.brazoIzq or { 0, 0 }, p.brazoDer or { 0, 0 }
  pegar(img, heroe, brazoIzq, bi[1], by + bi[2])
  pegar(img, heroe, brazoDer, bd[1], by + bd[2])
  local tri = Image(W, H, ColorMode.RGB)                       -- el tridente sigue a la mano izquierda
  pegar(tri, tridente, function() return true end, bi[1], by + bi[2])
  return img, tri
end

-- Caminar en 8 fotogramas (como Naia). l = cuánto se levanta la pierna en el paso,
-- b = cuánto baja el cuerpo al recibir el peso, a = balanceo de los brazos
local function caminar(l, b, a)
  local mitad = math.max(1, l // 2)
  return {
    { der = 0,     cuerpo = 0, brazoIzq = { 0,  a }, brazoDer = { 0, -a } },  -- 1 contacto
    { der = mitad, cuerpo = b, brazoIzq = { 0,  a }, brazoDer = { 0, -a } },  -- 2 recibe el peso
    { der = l,     cuerpo = 0 },                                                -- 3 paso (se cruzan)
    { der = mitad, cuerpo = 0, brazoIzq = { 0, -a }, brazoDer = { 0,  a } },  -- 4 subida
    { izq = 0,     cuerpo = 0, brazoIzq = { 0, -a }, brazoDer = { 0,  a } },  -- 5 contacto (otra pierna)
    { izq = mitad, cuerpo = b, brazoIzq = { 0, -a }, brazoDer = { 0,  a } },  -- 6
    { izq = l,     cuerpo = 0 },                                                -- 7
    { izq = mitad, cuerpo = 0, brazoIzq = { 0,  a }, brazoDer = { 0, -a } },  -- 8
  }
end

-- Saltar en 7 fotogramas (como Naia). Se usa por partes en el juego:
-- 2-3 subir, 4 cima, 5 caer, 6-7 aterrizar (el 1, impulso, es tan corto que se salta).
local function saltar(brazos, recoger)
  return {
    { cuerpo = 2, separar = 1, brazoIzq = { 0, 1 }, brazoDer = { 0, 1 } },                         -- 1 impulso (aplastado)
    { brazoIzq = { 0, -brazos }, brazoDer = { 0, -brazos } },                                        -- 2 despegue (estirado)
    { izq = 1, der = 1, brazoIzq = { 0, -brazos }, brazoDer = { 0, -brazos } },                      -- 3 subida
    { izq = recoger, der = recoger, brazoIzq = { 0, -2 }, brazoDer = { 0, -2 } },                    -- 4 cima: piernas recogidas, brazos arriba
    { izq = 1, der = 0, brazoIzq = { 0, -brazos + 1 }, brazoDer = { 0, -brazos + 1 } },              -- 5 caída
    { cuerpo = 3, separar = 1, brazoIzq = { 0, 1 }, brazoDer = { 0, 1 } },                           -- 6 aterrizaje (muy aplastado)
    { cuerpo = 1 },                                                                                   -- 7 recuperar
  }
end

local PROPUESTAS = {
  { animacion = "Caminar (como Naia)", descripcion =
      "8 fotogramas: contacto, recibir el peso, paso y subida, con cada pierna. Siempre un pie en el suelo; los brazos van al revés que las piernas y el tridente sigue a la mano.",
    variantes = {
      { id = "caminar_suave", nombre = "A · Paso suave", nota = "La pierna sube 2 px, el cuerpo baja 1 px y los brazos se mueven 1 px. Tranquilo, como el caminar de Naia.",
        poses = caminar(2, 1, 1), ms = 110 },
      { id = "caminar_marcado", nombre = "B · Paso marcado", nota = "La pierna sube 3 px y los brazos 2 px. Se lee mejor desde lejos, más enérgico.",
        poses = caminar(3, 1, 2), ms = 100 },
      { id = "caminar_rapido", nombre = "C · Paso rápido", nota = "Igual que B pero más rápido (80 ms por fotograma): casi un trote, va bien con la velocidad 5 del juego.",
        poses = caminar(3, 1, 2), ms = 80 },
    } },
  { animacion = "Saltar (como Naia)", descripcion =
      "7 fotogramas: impulso, despegue, subida, cima, caída, aterrizaje y recuperar. En el juego: 2-3 al subir, 4 en la cima, 5 al caer y 6-7 al aterrizar.",
    variantes = {
      { id = "saltar_suave", nombre = "A · Salto contenido", nota = "Brazos 2 px arriba y piernas recogidas 2 px en la cima.",
        poses = saltar(2, 2), ms = { 70, 90, 90, 140, 120, 80, 90 } },
      { id = "saltar_amplio", nombre = "B · Salto amplio", nota = "Brazos 3 px arriba y piernas recogidas 3 px: más estiramiento y aplastamiento, como las poses de Naia.",
        poses = saltar(3, 3), ms = { 70, 90, 90, 140, 120, 80, 90 } },
    } },
}

------------------------------------------------------------------ salida
local salida = app.fs.joinPath(dir, "revision")
local fh = assert(io.open(app.fs.joinPath(salida, "manifest.json")))
local manifiesto = json.decode(fh:read("a")); fh:close()
-- Quita versiones anteriores de estas mismas propuestas
local nuevas = {}
for _, p in ipairs(PROPUESTAS) do nuevas[p.animacion] = true end
-- Lo que devuelve json.decode no es una tabla de Lua: dentro de una tabla nueva se guardaría
-- como null y se perderían las animaciones ya aprobadas. Por eso se copia a tablas normales.
local function plano(v)
  if type(v) ~= "userdata" then return v end
  local t = {}
  for i, x in ipairs(v) do t[i] = plano(x) end           -- listas (pairs no las recorre)
  if #t == 0 then for k, x in pairs(v) do t[k] = plano(x) end end  -- objetos
  return t
end
manifiesto = plano(manifiesto)
local animaciones = {}
for _, a in ipairs(manifiesto.animaciones) do if not nuevas[a.nombre] then animaciones[#animaciones + 1] = a end end

for _, p in ipairs(PROPUESTAS) do
  local entrada = { nombre = p.animacion, descripcion = p.descripcion, variantes = {} }
  for _, v in ipairs(p.variantes) do
    local base = app.fs.joinPath(salida, "sprites", v.id)
    for _, sub in ipairs({ "", "heroe", "tridente" }) do app.fs.makeAllDirectories(app.fs.joinPath(base, sub)) end
    local fr, dur = {}, {}
    for i, ps in ipairs(v.poses) do
      local img, tri = pose(ps)
      local nombre = string.format("%02d.png", i)
      img:saveAs(app.fs.joinPath(base, "heroe", nombre))
      tri:saveAs(app.fs.joinPath(base, "tridente", nombre))
      local comb = img:clone(); comb:drawImage(tri, Point(0, 0), 255, BlendMode.NORMAL)
      comb:saveAs(app.fs.joinPath(base, nombre))
      fr[#fr + 1] = "sprites/" .. v.id .. "/" .. nombre
      dur[#dur + 1] = type(v.ms) == "table" and v.ms[i] or v.ms
    end
    entrada.variantes[#entrada.variantes + 1] = { id = v.id, nombre = v.nombre, nota = v.nota, fotogramas = fr, duraciones = dur }
  end
  animaciones[#animaciones + 1] = entrada
end
manifiesto.animaciones = animaciones
fh = assert(io.open(app.fs.joinPath(salida, "manifest.json"), "w"))
fh:write(json.encode(manifiesto)); fh:close()
print("ok", #PROPUESTAS, "animaciones")
