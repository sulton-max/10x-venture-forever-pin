#!/usr/bin/env python3
"""Compare Routed QR refinements using preserved pixels and actual Geist 700.

Requires Python 3 and Pillow with FreeType/WOFF2 support. No production asset
is written. Each run verifies a fresh temporary replay of both review sheets.
"""

from __future__ import annotations

import argparse
import hashlib
import json
import platform
import statistics
import tempfile
from pathlib import Path

from PIL import Image, ImageDraw, ImageFont, __version__ as pillow_version


HERE = Path(__file__).resolve().parent
FONT = HERE / "../fonts/geist-latin-wght-normal.woff2"
LICENSE = HERE / "../fonts/LICENSE"
ITEMS = [
    ("Original 01", "Selected direction / reference", HERE / "../source/01-routed-qr.png"),
    ("A Regularized", "Provisional geometry study", HERE / "source/01a-refined-v2.png"),
    ("B Open spacing", "Provisional spacing study", HERE / "source/01b-micro.png"),
]
INK = "#202331"
MUTED = "#586174"
LIGHT = "#f4f5f8"
DARK = "#14151a"
WHITE = "#ffffff"
CARD_WIDTH = 395
CARD_LEFT = [28, 451, 874]
SIZES = (16, 20, 24, 32)


def sha256(path: Path) -> str:
    return hashlib.sha256(path.read_bytes()).hexdigest()


def font(size: int, weight: int = 500) -> ImageFont.FreeTypeFont:
    result = ImageFont.truetype(str(FONT), size)
    result.set_variation_by_axes([weight])
    return result


def load_sources():
    images = []
    records = []
    for label, subtitle, path in ITEMS:
        with Image.open(path) as source:
            original_mode = source.mode
            image = source.convert("RGBA")
        alpha = image.getchannel("A")
        histogram = alpha.histogram()
        bounds = alpha.point(lambda value: 255 if value > 16 else 0).getbbox()
        if not bounds:
            raise ValueError(f"Empty artwork: {path}")
        if original_mode != "RGBA" or histogram[0] == 0:
            raise ValueError(f"Expected RGBA artwork with actual transparency: {path}")
        crop = image.crop(bounds)
        images.append((label, subtitle, crop))
        records.append({
            "label": label,
            "file": str(path.relative_to(HERE)),
            "sha256": sha256(path),
            "native_size": list(image.size),
            "native_mode": original_mode,
            "alpha_extrema": list(alpha.getextrema()),
            "alpha_zero_pixels": histogram[0],
            "alpha_partial_pixels": sum(histogram[1:255]),
            "alpha_opaque_pixels": histogram[255],
            "alpha_nonzero_bounds": list(alpha.getbbox()),
            "crop_bounds_alpha_greater_than_16": list(bounds),
            "crop_size": list(crop.size),
            "preserved_crop_rgba_sha256": hashlib.sha256(crop.tobytes()).hexdigest(),
        })
    return images, records


def fit(image: Image.Image, extent: int) -> Image.Image:
    scale = extent / max(image.size)
    if scale > 1:
        raise ValueError("Review artwork cannot be upscaled except for labeled pixel detail")
    size = (max(1, round(image.width * scale)), max(1, round(image.height * scale)))
    return image.resize(size, Image.Resampling.LANCZOS)


def put_center(canvas: Image.Image, image: Image.Image, box):
    x, y, width, height = box
    canvas.alpha_composite(image, (x + (width - image.width) // 2, y + (height - image.height) // 2))


def label(draw, xy, text, size=18, weight=500, fill=MUTED):
    draw.text(xy, text, font=font(size, weight), fill=fill)


def comparison(images):
    canvas = Image.new("RGBA", (1297, 1240), LIGHT)
    draw = ImageDraw.Draw(canvas)
    label(draw, (28, 22), "ForeverPin / Routed QR refinement", 40, 700, INK)
    label(draw, (28, 77), "Selected direction and two spacing studies. Production assets remain unchanged.", 21)
    label(draw, (28, 110), "Source colors and alpha preserved; real Geist 700. Violet remains approximate.", 18)
    for left, (name, subtitle, image) in zip(CARD_LEFT, images):
        draw.rounded_rectangle((left, 166, left + CARD_WIDTH, 1178), 20, fill=WHITE)
        label(draw, (left + 22, 190), name, 25, 700, INK)
        label(draw, (left + 22, 228), subtitle, 17)
        put_center(canvas, fit(image, 252), (left + 22, 280, 351, 274))
        label(draw, (left + 22, 580), "Equal maximum extent", 16)
        draw.line((left + 22, 625, left + 373, 625), fill="#e4e7ee")
        label(draw, (left + 22, 648), "Horizontal / Geist 700", 16)
        mark = fit(image, 46)
        text_font = font(35, 700)
        text_box = text_font.getbbox("ForeverPin")
        text_width = text_box[2] - text_box[0]
        start = left + (CARD_WIDTH - mark.width - 12 - text_width) // 2
        canvas.alpha_composite(mark, (start, 691 + (46 - mark.height) // 2))
        draw.text((start + mark.width + 12, 691 + (46 - (text_box[3] - text_box[1])) // 2 - text_box[1]),
                  "ForeverPin", font=text_font, fill=INK)
        label(draw, (left + 22, 775), "Light carrier / source on dark", 16)
        draw.rounded_rectangle((left + 35, 824, left + 173, 962), 27, fill="#eee8fc")
        put_center(canvas, fit(image, 96), (left + 35, 824, 138, 138))
        draw.rounded_rectangle((left + 221, 824, left + 359, 962), 27, fill=DARK)
        put_center(canvas, fit(image, 96), (left + 221, 824, 138, 138))
        label(draw, (left + 22, 1003), "Actual-pixel maximum extents", 16)
        for index, size in enumerate(SIZES):
            x = left + 30 + index * 89
            put_center(canvas, fit(image, size), (x, 1050, 56, 48))
            label(draw, (x + 6, 1113), f"{size}px", 16)
    label(draw, (28, 1201), "A and B are review candidates. Spacing targets and production minimums require verification.", 17)
    return canvas.convert("RGB")


def component_areas(image: Image.Image):
    alpha = image.getchannel("A")
    pixels = alpha.load()
    remaining = {(x, y) for y in range(alpha.height) for x in range(alpha.width) if pixels[x, y] >= 128}
    areas = []
    while remaining:
        start = remaining.pop()
        queue = [start]
        area = 0
        while queue:
            x, y = queue.pop()
            area += 1
            for point in ((x - 1, y), (x + 1, y), (x, y - 1), (x, y + 1)):
                if point in remaining:
                    remaining.remove(point)
                    queue.append(point)
        areas.append(area)
    return sorted(areas, reverse=True)


def separator_metrics(image: Image.Image):
    """Measure central QR gutters across straight midbody samples, not rounded caps."""
    pixels = image.getchannel("A").load()
    result = {"threshold": "alpha >= 128", "normalization": "maximum cropped visible extent",
              "sample_fractions": "17% through 33% of the perpendicular extent"}
    for axis in ("vertical", "horizontal"):
        widths = []
        for fraction in range(17, 34):
            if axis == "vertical":
                cross = round(image.height * fraction / 100)
                middle = image.width // 2
                dimension = image.width
                opacity = lambda position: pixels[position, cross]
            else:
                cross = round(image.width * fraction / 100)
                middle = image.height // 2
                dimension = image.height
                opacity = lambda position: pixels[cross, position]
            left = middle
            while left >= 0 and opacity(left) < 128:
                left -= 1
            right = middle
            while right < dimension and opacity(right) < 128:
                right += 1
            if left < 0 or right >= dimension:
                raise ValueError(f"Cannot bound the {axis} internal gutter")
            widths.append(max(0, right - left - 1))
        median = statistics.median(widths)
        result[axis] = {"min_px": min(widths), "median_px": median, "max_px": max(widths),
                        "median_percent_of_extent": round(median / max(image.size) * 100, 3)}
    return result


def size_review(images):
    canvas = Image.new("RGBA", (1297, 1220), LIGHT)
    draw = ImageDraw.Draw(canvas)
    label(draw, (28, 22), "ForeverPin / Routed QR at small sizes", 40, 700, INK)
    label(draw, (28, 78), "Native pixels beside magnified details. Inspect the native marks at 100%.", 20)
    label(draw, (28, 111), "Equal maximum visible extent; source proportions and alpha retained.", 18)
    metrics = []
    for left, (name, _, image) in zip(CARD_LEFT, images):
        draw.rounded_rectangle((left, 166, left + CARD_WIDTH, 1163), 20, fill=WHITE)
        label(draw, (left + 22, 188), name, 25, 700, INK)
        for row, size in enumerate(SIZES):
            y = 242 + row * 224
            mark = fit(image, size)
            label(draw, (left + 22, y + 4), f"{size}px", 21, 700, INK)
            put_center(canvas, mark, (left + 20, y + 52, 58, 58))
            draw.rounded_rectangle((left + 95, y + 12, left + 371, y + 205), 8, fill=LIGHT)
            enlarged = mark.resize((mark.width * 8, mark.height * 8), Image.Resampling.NEAREST)
            # At 32px the detail is 256px tall; keep the final row within the card.
            detail = 6 if size == 32 else 8
            if detail != 8:
                enlarged = mark.resize((mark.width * detail, mark.height * detail), Image.Resampling.NEAREST)
            put_center(canvas, enlarged, (left + 95, y + 12, 276, 193))
            label(draw, (left + 22, y + 152), f"{detail}x detail", 13)
            metrics.append({"label": name, "maximum_extent_px": size, "render_size": list(mark.size),
                            "foreground_component_areas_alpha_128": component_areas(mark)})
    label(draw, (28, 1184), "Pixel connectivity is evidence, not recognition testing or production approval.", 17)
    return canvas.convert("RGB"), metrics


def render(output: Path, images):
    output.mkdir(parents=True, exist_ok=True)
    sheets = {"comparison.png": comparison(images)}
    sheets["size-review.png"], metrics = size_review(images)
    records = []
    for filename, sheet in sheets.items():
        path = output / filename
        sheet.save(path, optimize=False)
        records.append({"file": filename, "size": list(sheet.size), "sha256": sha256(path)})
    return records, metrics


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--output-dir", type=Path, default=HERE)
    args = parser.parse_args()
    images, sources = load_sources()
    outputs, metrics = render(args.output_dir, images)
    with tempfile.TemporaryDirectory(prefix="foreverpin-logo-replay-") as temporary:
        repeated, repeated_metrics = render(Path(temporary), images)
        if repeated != outputs or repeated_metrics != metrics:
            raise RuntimeError("Temporary replay differs from saved review")
    manifest = {
        "schema": 1,
        "status": "refinement candidates; original 01 direction selected; no candidate approved",
        "sources": sources,
        "font": {"file": "../fonts/" + FONT.name, "sha256": sha256(FONT), "weight_axis": 700,
                 "license": "../fonts/LICENSE", "license_sha256": sha256(LICENSE)},
        "transforms": {"bounds": "alpha > 16; crop retains original RGBA inside those bounds",
                       "resampling": "LANCZOS", "scale": "equal maximum visible extent",
                       "artwork_recolored": False, "artwork_redrawn": False,
                       "palette": "original source values; generated colors are not certified brand tokens",
                       "detail": "nearest-neighbor enlargement labeled per row",
                       "lettering": "real Geist normal variable font, wght=700; no synthetic weight"},
        "runtime": {"python": platform.python_version(), "pillow": pillow_version},
        "requested_spacing_targets": {
            "A": "approximately 1/24 maximum visible extent; requires measurement",
            "B": "approximately 1/12 maximum visible extent; micro optical exception requiring review",
            "compliance_claimed": False,
        },
        "small_size_metrics": metrics,
        "measured_separator_widths": [{"label": name, **separator_metrics(image)}
                                     for name, _, image in images],
        "outputs": outputs,
        "verification": {"replay_verified": True, "method": "fresh temporary render; byte-identical PNG hashes"},
    }
    (args.output_dir / "manifest.json").write_text(json.dumps(manifest, indent=2) + "\n")
    print(json.dumps({"output": str(args.output_dir), "replay_verified": True, "sheets": outputs}, indent=2))


if __name__ == "__main__":
    main()
