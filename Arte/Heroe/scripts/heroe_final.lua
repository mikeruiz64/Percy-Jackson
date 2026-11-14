-- Monta los .aseprite finales (pixel art 45x70) con las animaciones aprobadas.
-- Lee los fotogramas y duraciones de revision/manifest.json.
--
-- Uso: aseprite -b --script-param dir=<carpeta Heroe> --script heroe_final.lua

local dir = app.params.dir
-- Cada etiqueta: { nombre, variante aprobada (id), subcarpeta de la capa (nil = la imagen principal),
-- primer y último fotograma (nil = todos) }.
-- Caminar = "C · Paso rápido" y saltar = "B · Salto amplio" (aprobados el 2026-11-14).
-- El salto se parte como en Naia: 2-3 subir, 4 cima, 5 caer, 6-7 aterrizar.
local function conMovimiento(primera, sub)
  return {
    primera,
    { "caminar", "caminar_rapido", sub },
    { "subir", "saltar_amplio", sub, 2, 3 },
    { "cima", "saltar_amplio", sub, 4, 4 },
    { "caer", "saltar_amplio", sub, 5, 5 },
    { "aterrizar", "saltar_amplio", sub, 6, 7 },
  }
end
local SALIDAS = {
  { archivo = "heroe.aseprite", capa = "Héroe", etiquetas = conMovimiento({ "reposo", "reposo" }, "heroe") },
  { archivo = "tridente/tridente.aseprite", capa = "Tridente", etiquetas = conMovimiento({ "brillo", "tridente_brillo" }, "tridente") },
}

local fh = assert(io.open(app.fs.joinPath(dir, "revision", "manifest.json")))
local manifiesto = json.decode(fh:read("a"))
fh:close()
local variantes = {}
for _, anim in ipairs(manifiesto.animaciones) do
  for _, v in ipairs(anim.variantes) do variantes[v.id] = v end
end

for _, s in ipairs(SALIDAS) do
  local spr = Sprite(manifiesto.ancho, manifiesto.alto, ColorMode.RGB)
  spr.layers[1].name = s.capa
  local f, rangos = 0, {}
  for _, e in ipairs(s.etiquetas) do
    local nombre, id, sub, desde, hasta = e[1], e[2], e[3], e[4], e[5]
    local v = assert(variantes[id], "falta la variante " .. id)
    local inicio = f + 1
    for i = desde or 1, hasta or #v.fotogramas do
      local ruta = v.fotogramas[i]
      if sub then ruta = ruta:gsub("([^/]+)$", sub .. "/%1") end   -- sprites/id/NN.png -> sprites/id/sub/NN.png
      f = f + 1
      if f > 1 then spr:newEmptyFrame() end
      spr:newCel(spr.layers[1], f, Image{ fromFile = app.fs.joinPath(dir, "revision", ruta) }, Point(0, 0))
      spr.frames[f].duration = v.duraciones[i] / 1000
    end
    rangos[#rangos + 1] = { nombre, inicio, f }
  end
  assert(f > 0, "no se encontró ningún fotograma para " .. s.archivo)
  -- Las etiquetas se crean al final: si no, crecen al añadir fotogramas detrás
  for _, r in ipairs(rangos) do spr:newTag(r[2], r[3]).name = r[1] end
  spr:saveAs(app.fs.joinPath(dir, s.archivo))
  spr:close()
  print(s.archivo, "fotogramas", f)
end
