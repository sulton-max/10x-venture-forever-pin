#!/usr/bin/env python3
"""Create and recheck a disposable pilot code against running ForeverPin services."""
import argparse
import http.client
import json
import os
from pathlib import Path
import urllib.request
import urllib.error
from urllib.parse import unquote, urlparse


class NoRedirect(urllib.request.HTTPRedirectHandler):
    def redirect_request(self, request, fp, code, message, headers, newurl):
        return None


def request(base, path, method="GET", payload=None, cookie=None, extra_headers=None):
    headers = {"Content-Type": "application/json", **(extra_headers or {})}
    if cookie:
        headers["Cookie"] = cookie
    body = json.dumps(payload).encode() if payload is not None else (b"" if method == "POST" else None)
    req = urllib.request.Request(base.rstrip("/") + path, data=body, headers=headers, method=method)
    try:
        return urllib.request.build_opener(NoRedirect).open(req, timeout=20)
    except urllib.error.HTTPError as response:
        return response


def keep_cookies(response, jar):
    """Keeps each Set-Cookie name and value; the stack is plain HTTP, so Secure attributes are ignored here."""
    for value in response.headers.get_all("Set-Cookie", []):
        name, _, content = value.split(";")[0].partition("=")
        jar[name.strip()] = content
    return "; ".join(name + "=" + content for name, content in jar.items())


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
        jar = {}
        with request(management, "/api/identity/guest", "POST") as response:
            assert response.status == 200, "Guest creation failed"
            cookie = keep_cookies(response, jar)
        assert "user-id" in jar, "Guest creation set no user cookie"
        # Cookie-authenticated writes echo the XSRF-TOKEN cookie, which a read issues for the signed-in guest.
        with request(management, "/api/runtime-config", cookie=cookie) as response:
            assert response.status == 200, "Runtime configuration failed"
            cookie = keep_cookies(response, jar)
        assert "XSRF-TOKEN" in jar, "The API issued no antiforgery token"
        antiforgery = {"X-XSRF-TOKEN": unquote(jar["XSRF-TOKEN"])}
        payload = {
            "name": "Wheelhouse deployment acceptance", "barcodeFormat": "QrCode", "mode": "dynamic",
            "contentType": "url", "rules": [{"type": "default", "content": {"type": "url", "url": "https://example.com/wheelhouse-pilot"}}],
            "style": {"foregroundColor": "#000000", "backgroundColor": "#FFFFFF", "transparentBackground": False,
                      "eccLevel": "Q", "quietZoneModules": 4, "moduleShape": "square", "finderShape": "square", "finderDotShape": "square"}
        }
        with request(management, "/api/codes", "POST", payload, cookie, antiforgery) as response:
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
        assert response.headers["Location"] == "https://example.com/wheelhouse-pilot", "Redirect destination changed"
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
