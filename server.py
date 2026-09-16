"""Minimal shared-position game server for the Unity assignment."""

import json
import os
import threading
import time
from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer

PLAYERS = {}
LOCK = threading.Lock()
MAX_BODY = 1024

PAGE = """<!doctype html><html lang=\"ja\"><meta charset=\"utf-8\"><title>UN42 Online Game</title>
<style>body{font:18px sans-serif;max-width:650px;margin:40px auto;padding:0 20px}button{font-size:24px;padding:12px 22px;margin:4px}pre{background:#eee;padding:16px}</style>
<h1>UN42 Online Game</h1><p>Render 上の Python サーバーです。Unity を起動すると、同じサーバーのプレイヤーが表示されます。</p>
<p>ブラウザーの操作は Unity 内の赤いキューブに反映されます。</p>
<div><button onclick=\"move(0,1)\">↑</button></div><div><button onclick=\"move(-1,0)\">←</button><button onclick=\"move(0,-1)\">↓</button><button onclick=\"move(1,0)\">→</button></div>
<pre id=\"state\">Loading...</pre><script>
async function move(dx,dz){await fetch('/move',{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify({id:'browser',dx,dz})});refresh()}
async function refresh(){try{const r=await fetch('/state');document.getElementById('state').textContent=JSON.stringify(await r.json(),null,2)}catch(e){document.getElementById('state').textContent=String(e)}}
setInterval(refresh,1000);refresh();</script></html>"""


class Handler(BaseHTTPRequestHandler):
    def send(self, status, data, content_type="application/json"):
        body = data.encode("utf-8")
        self.send_response(status)
        self.send_header("Content-Type", content_type + "; charset=utf-8")
        self.send_header("Content-Length", str(len(body)))
        self.send_header("Access-Control-Allow-Origin", "*")
        self.end_headers()
        self.wfile.write(body)

    def do_GET(self):
        if self.path == "/":
            return self.send(200, PAGE, "text/html")
        if self.path == "/health":
            return self.send(200, '{"status":"ok"}')
        if self.path == "/state":
            with LOCK:
                now = time.time()
                stale = [pid for pid, p in PLAYERS.items() if now - p["seen"] > 120]
                for pid in stale:
                    del PLAYERS[pid]
                players = [{"id": pid, "x": p["x"], "z": p["z"]} for pid, p in PLAYERS.items()]
            return self.send(200, json.dumps({"players": players}))
        self.send(404, '{"error":"not found"}')

    def do_POST(self):
        if self.path != "/move":
            return self.send(404, '{"error":"not found"}')
        try:
            length = int(self.headers.get("Content-Length", "0"))
            if length < 1 or length > MAX_BODY:
                raise ValueError("invalid length")
            data = json.loads(self.rfile.read(length))
            pid = data["id"]
            dx, dz = data["dx"], data["dz"]
            if not isinstance(pid, str) or not 1 <= len(pid) <= 40:
                raise ValueError("invalid id")
            if type(dx) not in (int, float) or type(dz) not in (int, float):
                raise ValueError("invalid direction")
            if dx not in (-1, 0, 1) or dz not in (-1, 0, 1):
                raise ValueError("direction must be -1, 0 or 1")
        except (ValueError, KeyError, TypeError, json.JSONDecodeError):
            return self.send(400, '{"error":"invalid request"}')
        with LOCK:
            p = PLAYERS.setdefault(pid, {"x": 0, "z": 0, "seen": 0})
            p["x"] = max(-20, min(20, p["x"] + dx))
            p["z"] = max(-20, min(20, p["z"] + dz))
            p["seen"] = time.time()
            position = {"id": pid, "x": p["x"], "z": p["z"]}
        self.send(200, json.dumps(position))


if __name__ == "__main__":
    port = int(os.environ.get("PORT", "10000"))
    print(f"Online game server listening on 0.0.0.0:{port}", flush=True)
    ThreadingHTTPServer(("0.0.0.0", port), Handler).serve_forever()
