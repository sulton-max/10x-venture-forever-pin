#!/usr/bin/env python3
"""Export the approved ForeverPin Routed QR raster family.

Requires Python 3, Pillow with WOFF2 support, and NumPy. Sources are read-only.
The original symbol contour and gaps are preserved; no micro geometry is made.
--output-dir replays the whole family without changing app or compatibility files.
"""

from __future__ import annotations

import argparse
import base64
from collections import deque
import hashlib
import json
import math
from pathlib import Path
import platform
import shutil
import statistics

import numpy as np
from PIL import Image, ImageDraw, ImageFilter, ImageFont, __version__ as PILLOW_VERSION


REPO = Path(__file__).resolve().parents[2]
BRAND = REPO / "product/brand/routed-qr"
SOURCE = BRAND / "source"
SOURCE_HASHES = {
    "approved-routed-qr.png": "932cfd76dcbe7f8d5b49ad98411512f751bf1ec7039944cb9a5bf903c1abfbf1",
    "geist-latin-wght-normal.woff2": "19f9c92546aa300c312235e3125af1b81394d8db9a4bc4a425cd5b641d2d54e1",
    "Geist-LICENSE": "71609cbb5c78b5870d712eab73a31d76622635c6ed034ab5cee3b9ecbda8685f",
}
COLORS = {"violet": "#7040d7", "lavender": "#aa8cff", "ink": "#202331", "white": "#ffffff"}
PADDING = 8
SIZES = (16, 20, 24, 32, 48, 64)


def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def font(size, weight=700):
    result = ImageFont.truetype(str(SOURCE / "geist-latin-wght-normal.woff2"), size)
    result.set_variation_by_axes([weight])
    return result


def components(mask):
    pending = mask.copy()
    counts = []
    height, width = pending.shape
    while pending.any():
        y, x = np.argwhere(pending)[0]
        queue = deque([(int(x), int(y))])
        pending[y, x] = False
        count = 0
        while queue:
            x, y = queue.popleft()
            count += 1
            for nx, ny in ((x - 1, y), (x + 1, y), (x, y - 1), (x, y + 1)):
                if 0 <= nx < width and 0 <= ny < height and pending[ny, nx]:
                    pending[ny, nx] = False
                    queue.append((nx, ny))
        counts.append(count)
    return sorted(counts, reverse=True)


def gutters(alpha):
    pixels = alpha.load()
    results = {}
    for axis in ("vertical", "horizontal"):
        samples = []
        for fraction in range(17, 34):
            if axis == "vertical":
                cross, middle, dimension = round(alpha.height * fraction / 100), alpha.width // 2, alpha.width
                opacity = lambda position: pixels[position, cross]
            else:
                cross, middle, dimension = round(alpha.width * fraction / 100), alpha.height // 2, alpha.height
                opacity = lambda position: pixels[cross, position]
            left = right = middle
            while left >= 0 and opacity(left) < 128:
                left -= 1
            while right < dimension and opacity(right) < 128:
                right += 1
            if left < 0 or right >= dimension:
                raise ValueError(f"Unbounded {axis} separator")
            samples.append(max(0, right - left - 1))
        median = statistics.median(samples)
        results[axis] = {"min_px": min(samples), "median_px": median, "max_px": max(samples),
                         "percent_max_extent": round(median / max(alpha.size) * 100, 3)}
    return results


def canonical_alpha():
    image = Image.open(SOURCE / "approved-routed-qr.png").convert("RGBA")
    if image.size != (1254, 1254):
        raise ValueError("Unexpected approved source dimensions")
    original = image.getchannel("A")
    box = original.point(lambda value: 255 if value > 16 else 0).getbbox()
    original = original.crop(box)
    values = np.asarray(original)
    solid = values >= 128
    parts = components(solid)
    if len(parts) != 4:
        raise ValueError(f"Expected exactly four approved symbol parts; found {len(parts)}")
    mask = Image.fromarray(np.uint8(solid) * 255)
    interior = np.asarray(mask.filter(ImageFilter.MinFilter(3))) > 0
    edge_band = np.asarray(mask.filter(ImageFilter.MaxFilter(3))) > 0
    clean = values.copy()
    clean[~edge_band] = 0
    clean[interior] = 255
    result = Image.fromarray(clean)
    if not np.array_equal(clean >= 128, solid):
        raise ValueError("Cleanup changed the approved contour")
    before, after = gutters(original), gutters(result)
    if before != after:
        raise ValueError("Cleanup changed the approved internal gaps")
    nominal = 100 / 24
    for measurement in after.values():
        if not nominal * 0.9 <= measurement["percent_max_extent"] <= nominal * 1.1:
            raise ValueError("Approved gaps do not fit the recorded family tolerance")
    return result, {
        "source_dimensions": list(image.size), "crop_alpha_gt_16": list(box),
        "clean_alpha_size": list(result.size), "component_areas_alpha_128": parts,
        "alpha_128_contour_pixel_equality": True,
        "alpha_128_mask_sha256": hashlib.sha256(np.uint8(solid).tobytes()).hexdigest(),
        "alpha_changed_pixels": int(np.count_nonzero(clean != values)),
        "method": "original antialias edges retained within 1px contour band; eroded interiors made opaque; distant residue cleared",
        "geometry_modified": False, "gaps_unchanged": True, "gaps": after,
        "gap_measurement": "perpendicular central midbodies, 17%-33% cross-axis positions; alpha >= 128",
        "family_baseline_percent": nominal, "family_optical_tolerance_percent": 10,
        "alpha_raw_bytes_sha256": hashlib.sha256(result.tobytes()).hexdigest(),
    }


def ink(alpha, color):
    result = Image.new("RGBA", alpha.size, color)
    result.putalpha(alpha)
    return result


def fit(image, extent):
    if extent > max(image.size):
        raise ValueError("Approved raster artwork must not be upscaled")
    scale = extent / max(image.size)
    alpha = image.getchannel("A").resize((round(image.width * scale), round(image.height * scale)), Image.Resampling.LANCZOS)
    return ink(alpha, image.getpixel((0, 0))[:3])


def pad(image, amount=PADDING):
    result = Image.new("RGBA", (image.width + amount * 2, image.height + amount * 2))
    result.alpha_composite(image, (amount, amount))
    return result


def centered(canvas, image):
    canvas.alpha_composite(image, ((canvas.width - image.width) // 2, (canvas.height - image.height) // 2))
    return canvas


def lettering(color):
    face = font(200)
    text = "ForeverPin"
    tracking = -0.025 * 200
    positions = [face.getlength(text[:index + 1]) - face.getlength(character) + index * tracking
                 for index, character in enumerate(text)]
    boxes = [face.getbbox(character) for character in text]
    left = math.floor(min(x + bounds[0] for x, bounds in zip(positions, boxes)))
    right = math.ceil(max(x + bounds[2] for x, bounds in zip(positions, boxes)))
    top, bottom = min(bounds[1] for bounds in boxes), max(bounds[3] for bounds in boxes)
    result = Image.new("RGBA", (right - left, bottom - top))
    draw = ImageDraw.Draw(result)
    for x, character in zip(positions, text):
        draw.text((x - left, -top), character, font=face, fill=color)
    return result


def primary(symbol, name):
    height = 176
    width = round(symbol.width * height / symbol.height)
    mark = ink(symbol.getchannel("A").resize((width, height), Image.Resampling.LANCZOS), symbol.getpixel((0, 0))[:3])
    gap = 32
    result = Image.new("RGBA", (mark.width + gap + name.width, height))
    result.alpha_composite(mark)
    result.alpha_composite(name, (mark.width + gap, (height - name.height) // 2))
    return pad(result)


def tile(symbol, color):
    result = Image.new("RGBA", (1024, 1024))
    ImageDraw.Draw(result).rounded_rectangle((0, 0, 1023, 1023), radius=224, fill=color)
    return centered(result, fit(symbol, 720))


def favicon(symbol, size):
    margin = max(1, round(size / 16))
    return centered(Image.new("RGBA", (size, size)), fit(symbol, size - 2 * margin))


def paste_fit(canvas, image, box):
    left, top, width, height = box
    item = image.copy()
    item.thumbnail((width, height), Image.Resampling.LANCZOS)
    canvas.alpha_composite(item, (left + (width - item.width) // 2, top + (height - item.height) // 2))


def label(draw, position, text, size=20, color="#202331"):
    draw.text(position, text, font=font(size, 500), fill=color)


def preview(assets, output):
    sheet = Image.new("RGBA", (1480, 1140), "#f4f5f8")
    draw = ImageDraw.Draw(sheet)
    label(draw, (28, 22), "ForeverPin / approved Routed QR family", 34)
    label(draw, (28, 72), "Original geometry and gaps. Flat palette. Real Geist 700. Canonical raster assets.", 21)
    cells = [
        ("primary-light.png", "Primary / light", (24, 120, 704, 210), False),
        ("primary-dark.png", "Primary / dark", (752, 120, 704, 210), True),
        ("primary-mono-ink.png", "Primary / one ink", (24, 352, 704, 186), False),
        ("primary-mono-white.png", "Primary / white", (752, 352, 704, 186), True),
        ("symbol-violet.png", "Symbol / violet", (24, 560, 340, 260), False),
        ("symbol-lavender.png", "Symbol / lavender", (388, 560, 340, 260), True),
        ("tile-light.png", "Light tile", (752, 560, 340, 260), False),
        ("tile-dark.png", "Dark tile", (1116, 560, 340, 260), True),
        ("wordmark-ink.png", "Lettering component / ink", (24, 844, 704, 210), False),
        ("wordmark-white.png", "Lettering component / white", (752, 844, 704, 210), True),
    ]
    for name, title, (left, top, width, height), dark in cells:
        draw.rounded_rectangle((left, top, left + width, top + height), 16, fill="#14151a" if dark else "white")
        label(draw, (left + 20, top + 16), title, color="white" if dark else "#202331")
        paste_fit(sheet, assets[name], (left + 34, top + 62, width - 68, height - 84))
    label(draw, (28, 1082), "No gap widening or alternative micro symbol. Standalone minimum: 24px of visible artwork.", 20)
    sheet.convert("RGB").save(output / "preview.png")


def size_review(symbols, assets, output):
    sheet = Image.new("RGBA", (1480, 1080), "#f4f5f8")
    draw = ImageDraw.Draw(sheet)
    label(draw, (28, 22), "ForeverPin / actual-size review", 34)
    label(draw, (28, 72), "Inspect at 100%; 2x pixel detail below. Original gaps remain; 16px and 20px merge modules.", 20)
    metrics = []
    for row, (variant, background, foreground) in enumerate((("violet", "white", "#202331"),
                                                          ("lavender", "#14151a", "white"))):
        top = 120 + row * 340
        draw.rounded_rectangle((24, top, 1456, top + 312), 14, fill=background)
        for column, size in enumerate(SIZES):
            x = 46 + column * 232
            label(draw, (x, top + 16), f"Symbol {size}px", 18, foreground)
            mark = fit(symbols[variant], size)
            sheet.alpha_composite(mark, (x, top + 54))
            detail = mark.resize((mark.width * 2, mark.height * 2), Image.Resampling.NEAREST)
            sheet.alpha_composite(detail, (x, top + 136))
            metrics.append({"variant": variant, "visible_max_extent_px": size, "size": list(mark.size),
                            "components_alpha_128": len(components(np.asarray(mark.getchannel("A")) >= 128))})
    draw.rounded_rectangle((24, 800, 1456, 1040), 14, fill="white")
    for column, height in enumerate((24, 32, 40, 48)):
        x = 46 + column * 350
        label(draw, (x, 818), f"Primary {height}px high", 18)
        image = assets["primary-light.png"]
        mark = image.resize((round(image.width * height / image.height), height), Image.Resampling.LANCZOS)
        sheet.alpha_composite(mark, (x, 856))
    for column, size in enumerate((16, 32, 48)):
        x = 50 + column * 160
        label(draw, (x, 940), f"Favicon {size}px", 16)
        sheet.alpha_composite(assets[f"favicon-{size}.png"], (x, 977))
    for column, name in enumerate(("foreverpin-navbar-logo.png", "foreverpin-navbar-logo@2x.png")):
        x = 750 + column * 330
        label(draw, (x, 940), "Navbar at 28 CSS px", 16)
        sheet.alpha_composite(assets[name].resize((28, 28), Image.Resampling.LANCZOS), (x, 977))
    sheet.convert("RGB").save(output / "size-review.png")
    return metrics


def mask_review(symbols, assets, output):
    sheet = Image.new("RGBA", (1440, 660), "#f4f5f8")
    draw = ImageDraw.Draw(sheet)
    label(draw, (28, 22), "ForeverPin / tile mask review", 34)
    label(draw, (28, 72), "Circle and squircle probes preserve all artwork; they are not platform packaging certification.", 20)
    y, x = np.indices((1024, 1024))
    nx, ny = np.abs((x - 511.5) / 512), np.abs((y - 511.5) / 512)
    masks = {"Circle": nx * nx + ny * ny <= 1, "Squircle": nx ** 4 + ny ** 4 <= 1}
    metrics = []
    for row, (variant, name) in enumerate((("violet", "tile-light.png"), ("lavender", "tile-dark.png"))):
        top = 128 + row * 250
        artwork = centered(Image.new("RGBA", (1024, 1024)), fit(symbols[variant], 720))
        for column, (mask_name, mask) in enumerate(masks.items()):
            left = 40 + column * 700
            trimmed = assets[name].copy()
            trimmed.putalpha(Image.fromarray(np.asarray(trimmed.getchannel("A")) * np.uint8(mask)))
            lost = int(np.count_nonzero((np.asarray(artwork.getchannel("A")) >= 128) & ~mask))
            if lost:
                raise ValueError(f"{name} loses artwork under {mask_name}")
            label(draw, (left, top + 14), f"{name} / {mask_name}", 19)
            paste_fit(sheet, trimmed, (left + 382, top, 200, 200))
            label(draw, (left, top + 55), "Symbol pixels clipped: 0", 18)
            metrics.append({"asset": name, "mask": mask_name, "symbol_pixels_alpha_128_clipped": lost})
    sheet.convert("RGB").save(output / "mask-review.png")
    return metrics


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--output-dir", type=Path, default=BRAND)
    args = parser.parse_args()
    for name, expected in SOURCE_HASHES.items():
        if digest(SOURCE / name) != expected:
            parser.error(f"Approved input hash mismatch: {name}")
    output = args.output_dir.expanduser().resolve()
    exports = output / "exports"
    exports.mkdir(parents=True, exist_ok=True)
    (output / "source").mkdir(exist_ok=True)
    for name in SOURCE_HASHES:
        destination = output / "source" / name
        if destination.resolve() != (SOURCE / name).resolve():
            shutil.copy2(SOURCE / name, destination)
    alpha, invariants = canonical_alpha()
    alpha.save(output / "source/clean-symbol-alpha.png")
    symbols = {name: ink(alpha, color) for name, color in COLORS.items()}
    names = {name: lettering(COLORS[name]) for name in ("ink", "white")}
    assets = {f"symbol-{name}.png": pad(image) for name, image in symbols.items()}
    assets.update({f"wordmark-{name}.png": pad(image) for name, image in names.items()})
    for name, symbol_color, text_color in (("light", "violet", "ink"), ("dark", "lavender", "white"),
                                          ("mono-ink", "ink", "ink"), ("mono-white", "white", "white")):
        text = lettering("#f1f3f8") if name == "dark" else names[text_color]
        composed = primary(symbols[symbol_color], text)
        if name.startswith("mono-"):
            composed = ink(composed.getchannel("A"), COLORS[text_color])
        assets[f"primary-{name}.png"] = composed
    assets["tile-light.png"] = tile(symbols["white"], COLORS["violet"])
    assets["tile-dark.png"] = tile(ink(alpha, "#1d1433"), COLORS["lavender"])
    assets["foreverpin-logo.png"] = assets["tile-light.png"]
    for name, size in (("foreverpin-google-logo.png", 120), ("apple-touch-icon.png", 180)):
        assets[name] = assets["tile-light.png"].resize((size, size), Image.Resampling.LANCZOS)
    for name, size, extent in (("foreverpin-navbar-logo.png", 56, 48),
                               ("foreverpin-navbar-logo@2x.png", 112, 96)):
        assets[name] = centered(Image.new("RGBA", (size, size)), fit(symbols["violet"], extent))
    icon_frames = {size: favicon(symbols["violet"], size) for size in (16, 32, 48, 64, 128, 256)}
    for size in (16, 32, 48, 256):
        assets[f"favicon-{size}.png"] = icon_frames[size]
    inventory = []
    for name, image in sorted(assets.items()):
        image.save(exports / name)
        inventory.append({"file": "exports/" + name, "size": list(image.size), "mode": image.mode,
                          "alpha_bbox": list(image.getchannel("A").getbbox()), "sha256": digest(exports / name)})
    icon_frames[256].save(exports / "favicon.ico", format="ICO", sizes=[(n, n) for n in icon_frames],
                          append_images=[icon_frames[n] for n in icon_frames if n != 256])
    with Image.open(exports / "favicon.ico") as icon:
        for size, original in icon_frames.items():
            if icon.ico.getimage((size, size)).convert("RGBA").tobytes() != original.tobytes():
                raise ValueError(f"ICO changed the {size}px source frame")
    for wrapper, raster in (("favicon.svg", "symbol-violet.png"), ("foreverpin-logo.svg", "foreverpin-logo.png")):
        encoded = base64.b64encode((exports / raster).read_bytes()).decode("ascii")
        width, height = assets[raster].size
        svg = (f'<svg xmlns="http://www.w3.org/2000/svg" width="{width}" height="{height}" viewBox="0 0 {width} {height}">\n'
               '<title>ForeverPin Routed QR</title>\n'
               '<metadata>Raster wrapper: approved PNG artwork embedded unchanged. This is not a vector master.</metadata>\n'
               f'<image width="{width}" height="{height}" href="data:image/png;base64,{encoded}"/>\n</svg>\n')
        (exports / wrapper).write_text(svg)
    preview(assets, output)
    small = size_review(symbols, assets, output)
    masks = mask_review(symbols, assets, output)
    for name in ("favicon.ico", "favicon.svg", "foreverpin-logo.svg"):
        inventory.append({"file": "exports/" + name, "sha256": digest(exports / name)})
    all_paths = [output / row["file"] for row in inventory]
    all_paths += [output / "source" / name for name in SOURCE_HASHES]
    all_paths += [output / name for name in ("source/clean-symbol-alpha.png", "preview.png", "size-review.png", "mask-review.png")]
    manifest = {
        "schema": 1, "identity": "ForeverPin Routed QR", "approved_date": "2026-09-29",
        "status": "approved original artwork; deterministic raster family; no app synchronization performed",
        "sources": [{"file": "source/" + name, "sha256": value} for name, value in SOURCE_HASHES.items()],
        "raster_only": True, "svg_wrappers": "embedded PNG wrappers, not vector geometry",
        "palette": COLORS, "primary_dark_lettering": "#f1f3f8",
        "tile_carriers": {"light": "#7040d7", "dark": "#aa8cff"},
        "tile_glyphs": {"light": "#ffffff", "dark": "#1d1433"},
        "symbol_invariants": invariants,
        "lettering": {"font": "Geist", "axis_wght": 700, "font_size_px": 200, "content": "ForeverPin",
                      "tracking_em": -0.025, "tracking_px": -5,
                      "tracking_method": "kerning-aware prefix advances plus per-character tracking; no synthetic bold",
                      "production_geometry": "rendered raster"},
        "composition": {"primary_symbol_height": 176, "symbol_text_gap": 32, "export_padding": PADDING,
                        "tile_size": 1024, "tile_symbol_max_extent": 720, "tile_corner_radius": 224},
        "usage": {"standalone_min_visible_extent_px": 24, "primary_reviewed_min_total_height_px": 32,
                  "clear_space": "one quarter of visible symbol height around compositions; external layout space",
                  "micro_geometry": "none; original gaps retained at all sizes",
                  "favicon_16_limitation": "ordinary downsampling merges modules; no 16px separated-module claim",
                  "navbar": "56/112px standalone PNGs show 24px visible artwork when displayed at 28 CSS pixels",
                  "native_platform_packaging": "circle/squircle artwork containment checked; store packaging not certified"},
        "runtime": {"python": platform.python_version(), "pillow": PILLOW_VERSION, "numpy": np.__version__},
        "assets": inventory, "small_size_metrics": small, "mask_checks": masks,
        "files": [{"file": str(path.relative_to(output)), "sha256": digest(path)} for path in sorted(all_paths)],
        "replay": {"command": "python3 engineering/scripts/export-brand-logos.py --output-dir /tmp/foreverpin-brand-replay",
                   "verification": "compare saved files and manifest byte for byte after independent replay"},
    }
    (output / "manifest.json").write_text(json.dumps(manifest, indent=2) + "\n")
    print(json.dumps({"output": str(output), "assets": [item["file"] for item in inventory],
                      "geometry_preserved": True, "small_size_metrics": small}, indent=2))


if __name__ == "__main__":
    main()
