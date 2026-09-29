#!/usr/bin/env python3
"""Copy the approved logo exports to their existing product and application paths.

Run export-brand-logos.py first. --check verifies copies without writing files.
The original QR family is archived separately; exploration files are not inputs.
"""

from __future__ import annotations

import argparse
import hashlib
import json
import shutil
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
BRAND = ROOT / "product/brand"
EXPORTS = BRAND / "routed-qr/exports"
APP = ROOT / "engineering/codebase/forever-pin.frontend-services/apps/web"
CURRENT_NAMES = (
    "foreverpin-logo.png", "foreverpin-logo.svg", "foreverpin-google-logo.png",
    "foreverpin-navbar-logo.png", "foreverpin-navbar-logo@2x.png",
    "favicon.svg", "favicon.ico", "favicon-16.png", "favicon-32.png",
    "favicon-48.png", "favicon-256.png", "apple-touch-icon.png",
)
APP_BRAND_NAMES = (
    "foreverpin-logo.png", "foreverpin-navbar-logo.png", "foreverpin-navbar-logo@2x.png",
)
PUBLIC_NAMES = (
    "favicon.svg", "favicon.ico", "favicon-16.png", "favicon-32.png",
    "favicon-48.png", "apple-touch-icon.png",
)


def digest(path: Path) -> str:
    return hashlib.sha256(path.read_bytes()).hexdigest()


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--check", action="store_true")
    args = parser.parse_args()
    destinations = [(name, BRAND / "current" / name) for name in CURRENT_NAMES]
    destinations += [(name, APP / "src/assets/brand" / name) for name in APP_BRAND_NAMES]
    destinations += [(name, APP / "public" / name) for name in PUBLIC_NAMES]
    destinations.append((
        "foreverpin-navbar-logo@2x.png",
        APP / "src/presentation/marketing/assets/foreverpin-navbar-logo@2x.png",
    ))

    # Validate the entire source set before replacing any active copy.
    approved = json.loads((BRAND / "routed-qr/manifest.json").read_text())
    recorded_hashes = {entry["file"]: entry["sha256"] for entry in approved["files"]}
    for name, _ in destinations:
        source = EXPORTS / name
        expected_hash = recorded_hashes.get(f"exports/{name}")
        if expected_hash is None or not source.is_file() or digest(source) != expected_hash:
            raise ValueError(f"Export is missing or differs from the approved manifest: {name}")

    records = []
    for name, destination in destinations:
        source = EXPORTS / name
        source_hash = digest(source)
        if not args.check:
            destination.parent.mkdir(parents=True, exist_ok=True)
            shutil.copyfile(source, destination)
        if not destination.is_file() or digest(destination) != source_hash:
            raise ValueError(f"Logo copy differs: {destination.relative_to(ROOT)}")
        records.append({
            "source": str(source.relative_to(ROOT)),
            "destination": str(destination.relative_to(ROOT)),
            "sha256": source_hash,
        })

    manifest = {
        "identity": "ForeverPin Routed QR",
        "approved_source": "product/brand/routed-qr/manifest.json",
        "approved_manifest_sha256": digest(BRAND / "routed-qr/manifest.json"),
        "copies": records,
        "external_provider_upload_performed": False,
    }
    manifest_path = BRAND / "current/manifest.json"
    serialized = json.dumps(manifest, indent=2) + "\n"
    if args.check:
        if manifest_path.read_text() != serialized:
            raise ValueError("The active-copy manifest is stale")
    else:
        manifest_path.write_text(serialized)
    print(f"Verified {len(records)} exact logo copies; provider upload remains separate.")


if __name__ == "__main__":
    main()
