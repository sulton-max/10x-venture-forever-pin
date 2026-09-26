#!/usr/bin/env python3
"""Create and recheck a disposable pilot code against running ForeverPin services."""
import argparse
import http.client
import json
import os
from pathlib import Path
import urllib.request
import urllib.error
from urllib.parse import urlparse


class NoRedirect(urllib.request.HTTPRedirectHandler):
    def redirect_request(self, request, fp, code, message, headers, newurl):
        return None


def request(base, path, method="GET", payload=None, cookie=None):
    headers = {"Content-Type": "application/json"}
    if cookie:
        headers["Cookie"] = cookie
    body = json.dumps(payload).encode() if payload is not None else (b"" if method == "POST" else None)
    req = urllib.request.Request(base.rstrip("/") + path, data=body, headers=headers, method=method)
    try:
        return urllib.request.build_opener(NoRedirect).open(req, timeout=20)
    except urllib.error.HTTPError as response:
        return response


def foreign_host_status(base):
    # A Host header outside AllowedHosts must be refused, or a healthy result proves nothing about filtering.
    address = urlparse(base)
    connection = http.client.HTTPConnection(address.hostname, address.port, timeout=20)
    try:
        connection.request("GET", "/health", headers={"Host": "foreign.invalid"})
        return connection.getresponse().status
    finally:
        connection.close()


def run(management, redirect, state_file, create=False):
    for base in (management, redirect):
        with request(base, "/health") as response:
            assert response.status == 200, "Service is not ready"
        assert foreign_host_status(base) == 400, "Host filtering accepted an unknown host"
    if create:
        with request(management, "/api/identity/guest", "POST") as response:
            assert response.status == 200, "Guest creation failed"
            cookie = next(value.split(";")[0] for value in response.headers.get_all("Set-Cookie", [])
                          if value.startswith("user-id="))
        payload = {
            "name": "DryDock deployment acceptance", "barcodeFormat": "QrCode", "mode": "dynamic",
            "contentType": "url", "rules": [{"type": "default", "content": {"type": "url", "url": "https://example.com/drydock-pilot"}}],
            "style": {"foregroundColor": "#000000", "backgroundColor": "#FFFFFF", "transparentBackground": False,
                      "eccLevel": "Q", "quietZoneModules": 4, "moduleShape": "square", "finderShape": "square", "finderDotShape": "square"}
        }
        with request(management, "/api/codes", "POST", payload, cookie) as response:
            assert response.status == 200, "Code creation failed"
            code = json.load(response)["data"]
        state = {"cookie": cookie, "id": code["id"], "slug": code["slug"]}
        descriptor = os.open(state_file, os.O_WRONLY | os.O_CREAT | os.O_TRUNC, 0o600)
        with os.fdopen(descriptor, "w") as stream:
            json.dump(state, stream)
    else:
        state = json.loads(Path(state_file).read_text())
    with request(management, "/api/codes", cookie=state["cookie"]) as response:
        assert response.status == 200, "Owner cannot list saved codes"
        assert any(code["id"] == state["id"] for code in json.load(response)["data"]), "Saved code did not persist"
    with request(management, "/api/codes/" + state["id"] + "/image?format=svg", cookie=state["cookie"]) as response:
        assert response.status == 200 and b"<svg" in response.read(), "Code image rendering failed"
    with request(redirect, "/" + state["slug"]) as response:
        assert response.status == 302, "Saved code did not redirect"
        assert response.headers["Location"] == "https://example.com/drydock-pilot", "Redirect destination changed"
    print(json.dumps({"status": "passed", "checks": ["readiness", "host-filter", "guest-owned-code", "svg", "redirect"],
                      "mode": "create" if create else "persistence"}))


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--management-url", required=True)
    parser.add_argument("--redirect-url", required=True)
    parser.add_argument("--state-file", required=True)
    parser.add_argument("--create", action="store_true")
    args = parser.parse_args()
    run(args.management_url, args.redirect_url, args.state_file, args.create)
