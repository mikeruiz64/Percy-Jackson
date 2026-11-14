"""Exporta el arte del Prólogo (estilo Amanecer dorado) a Assets/Sprites/Mapa.

Abre Arte/Mapa/exportar.html con Brave sin ventana, lee el JSON que deja en la
página y guarda nivel.png, pergamino.png, santuario.png y mapa.json (pivotes).
Uso, desde la carpeta del proyecto:  python -I Arte\\Mapa\\exportar_mapa.py
"""
import base64
import html
import json
import os
import pathlib
import re
import subprocess

RAIZ = pathlib.Path(__file__).resolve().parents[2]
PAGINA = (RAIZ / "Arte/Mapa/exportar.html").as_uri()
SALIDA = RAIZ / "Assets/Sprites/Mapa"
BRAVE = pathlib.Path(os.environ["LOCALAPPDATA"]) / "BraveSoftware/Brave-Browser/Application/brave.exe"

dom = subprocess.run(
    [str(BRAVE), "--headless=new", "--disable-gpu", "--virtual-time-budget=3000", "--dump-dom", PAGINA],
    capture_output=True, text=True, encoding="utf-8", timeout=120, check=True).stdout
encontrado = re.search(r'<pre id="salida">(.*?)</pre>', dom, re.S)
if not encontrado or not encontrado.group(1).strip():
    raise SystemExit("La página no devolvió datos: revisa exportar.html en Brave (F12).")
datos = json.loads(html.unescape(encontrado.group(1)))

SALIDA.mkdir(parents=True, exist_ok=True)
pivotes = {}
for nombre, pieza in datos.items():
    cabecera, b64 = pieza["png"].split(",", 1)
    assert cabecera == "data:image/png;base64", cabecera
    (SALIDA / f"{nombre}.png").write_bytes(base64.b64decode(b64))
    pivotes[nombre] = pieza["pivote"]
    print(f"{nombre}.png  pivote={pieza['pivote']}")
(SALIDA / "mapa.json").write_text(json.dumps(pivotes, indent=2), encoding="utf-8")
