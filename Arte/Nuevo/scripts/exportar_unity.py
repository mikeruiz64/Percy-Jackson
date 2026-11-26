"""Exporta las animaciones limpias del protagonista (versión B) al proyecto de Unity.

Abre Arte/Nuevo/revision/daga.html?exportar en Brave sin ventana, que recorta y limpia
las hojas de Assets/Sprites/Heroe/WhatsApp*.jpeg, y guarda en Assets/Sprites/Heroe/
una hoja PNG + JSON (formato de Aseprite) por animación:
  heroe_<animación>           con daga (arma inicial)
  heroe_tridente_<animación>  con tridente
Después, en Unity: Herramientas > Construir Percy Jackson.

Uso: python -I Arte/Nuevo/scripts/exportar_unity.py   (variable BRAVE para otro navegador)
"""
import base64
import functools
import html
import http.server
import json
import os
import re
import subprocess
import threading

PROYECTO = os.path.dirname(os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__)))))
SPRITES = os.path.join(PROYECTO, "Assets", "Sprites", "Heroe")
BRAVE = os.environ.get("BRAVE", os.path.expandvars(r"%LOCALAPPDATA%\BraveSoftware\Brave-Browser\Application\brave.exe"))


class Silencioso(http.server.SimpleHTTPRequestHandler):
    def log_message(self, *args):
        pass


def main():
    # Servidor local: el navegador solo deja leer los píxeles de imágenes servidas por http.
    manejador = functools.partial(Silencioso, directory=PROYECTO)
    servidor = http.server.ThreadingHTTPServer(("127.0.0.1", 0), manejador)
    threading.Thread(target=servidor.serve_forever, daemon=True).start()
    url = f"http://127.0.0.1:{servidor.server_port}/Arte/Nuevo/revision/daga.html?exportar"
    try:
        dom = subprocess.run([BRAVE, "--headless=new", "--disable-gpu", "--virtual-time-budget=20000", "--dump-dom", url],
                             capture_output=True, text=True, encoding="utf-8", timeout=180).stdout
    finally:
        servidor.shutdown()
    m = re.search(r'<pre id="salida">(.*?)</pre>', dom, re.S)
    if not m:
        raise SystemExit("La página no devolvió la exportación (¿cargaron las hojas?).")
    hojas = json.loads(html.unescape(m.group(1)))
    assert len(hojas) == 30, f"se esperaban 30 hojas y hay {len(hojas)}"
    os.makedirs(SPRITES, exist_ok=True)
    for nombre, datos in hojas.items():
        with open(os.path.join(SPRITES, nombre + ".png"), "wb") as f:
            f.write(base64.b64decode(datos["png"].split(",", 1)[1]))
        with open(os.path.join(SPRITES, nombre + ".json"), "w", encoding="utf-8") as f:
            json.dump(datos["json"], f, indent=1, ensure_ascii=False)
        print("ok", nombre, len(datos["json"]["frames"]), "fotogramas")


if __name__ == "__main__":
    main()
