#!/usr/bin/env python3
"""Create ForeverPin's immutable two-service deployment bundle from CI image digests."""
import argparse
import hashlib
import json
from pathlib import Path
import re


def create_bundle(output, management, redirect, release, commit, platform, rollback_compatible=False):
    for image in (management, redirect):
        if not re.fullmatch(r"[a-z0-9][a-z0-9./:_-]*@sha256:[a-f0-9]{64}", image):
            raise ValueError("Both images must use SHA-256 digests")
    if not re.fullmatch(r"[a-f0-9]{40}", commit):
        raise ValueError("A full source commit is required")
    if not re.fullmatch(r"[A-Za-z0-9][A-Za-z0-9._-]{0,95}", release):
        raise ValueError("Invalid release label")
    if platform not in ("linux/amd64", "linux/arm64"):
        raise ValueError("Unsupported platform")
    services = {}
    for name, image, memory in (("management", management, "768m"), ("redirect", redirect, "384m")):
        services[name] = {
            "image": image, "platform": platform, "restart": "unless-stopped", "mem_limit": memory,
            "init": True, "stop_grace_period": "45s",
            "security_opt": ["no-new-privileges:true"], "cap_drop": ["ALL"],
            "volumes": [{
                "type": "bind",
                "source": "${" + name.upper() + "_SETTINGS:?Set " + name.upper() + "_SETTINGS}",
                "target": "/app/appsettings.Local.json",
                "read_only": True,
            }],
            "healthcheck": {"test": ["CMD", "curl", "--fail", "--silent", "http://localhost:8080/health"],
                            "interval": "10s", "timeout": "5s", "start_period": "60s", "retries": 6},
            "networks": {"platform": {"aliases": ["foreverpin-${DEPLOY_ENVIRONMENT}-" + name]}},
            "logging": {"driver": "json-file", "options": {"max-size": "10m", "max-file": "3"}}
        }
    services["management"]["volumes"].append({"type": "volume", "source": "keys", "target": "/data/keys"})
    compose = {"services": services, "volumes": {"keys": {}},
               "networks": {"platform": {"external": True, "name": "${PLATFORM_NETWORK:?Set PLATFORM_NETWORK}"}}}
    compose_bytes = (json.dumps(compose, indent=2) + "\n").encode()
    manifest = {
        "schemaVersion": 1, "product": "foreverpin", "release": release, "sourceCommit": commit,
        "platform": platform, "rollbackCompatible": rollback_compatible,
        "composeSha256": hashlib.sha256(compose_bytes).hexdigest(),
        "images": {"management": management, "redirect": redirect},
        "requiredConfiguration": {
            "management": [
                "DatabaseOptions:ConnectionString",
                "ApiSettings:RedirectBaseUrl",
                "Auth:Google:ClientId",
                "Billing:SecretKey",
                "Billing:WebhookSecret",
                "Billing:Prices:Solo",
                "Billing:Prices:Pro",
                "Billing:Prices:Agency",
                "Billing:SuccessUrl",
                "Billing:CancelUrl",
                "AllowedHosts",
                "Deployment:TrustedProxies",
            ],
            "redirect": [
                "DatabaseOptions:ConnectionString",
                "AllowedHosts",
                "Deployment:TrustedProxies",
            ],
        },
        "healthProbes": {"management": "/health", "redirect": "/health"},
    }
    output = Path(output)
    output.mkdir(parents=True, exist_ok=False)
    (output / "compose.json").write_bytes(compose_bytes)
    (output / "release.json").write_text(json.dumps(manifest, indent=2) + "\n")
    return manifest


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--management-image", required=True)
    parser.add_argument("--redirect-image", required=True)
    parser.add_argument("--release", required=True)
    parser.add_argument("--commit", required=True)
    parser.add_argument("--platform", default="linux/amd64")
    parser.add_argument("--output", required=True)
    parser.add_argument("--rollback-compatible", action="store_true",
                        help="Only after reviewing migrations against the previous images")
    args = parser.parse_args()
    create_bundle(args.output, args.management_image, args.redirect_image, args.release, args.commit,
                  args.platform, args.rollback_compatible)
