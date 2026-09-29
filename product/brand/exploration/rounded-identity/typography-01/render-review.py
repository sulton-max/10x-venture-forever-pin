#!/usr/bin/env python3
"""Render a lettering-only study. This never writes to the approved family."""

from __future__ import annotations

from pathlib import Path

from PIL import Image, ImageDraw, ImageFont


HERE = Path(__file__).resolve().parent
BRAND = HERE.parents[2]
SOURCE = HERE / "source"
GEIST = BRAND / "routed-qr/source/geist-latin-wght-normal.woff2"
SYMBOL = BRAND / "routed-qr/source/clean-symbol-alpha.png"
WOW2 = SOURCE / "wow2-primary-navy.png"
WHEELHOUSE = SOURCE / "wheelhouse-wordmark.png"

OPTIONS = (
    ("00  Previous", "Geist 700 / ForeverPin", GEIST, (700,), "ForeverPin", -0.025),
    ("01  Dense", "Geist 900 / foreverpin", GEIST, (900,), "foreverpin", -0.035),
    ("02  Selected", "Nunito Sans 900 / foreverpin", SOURCE / "NunitoSans-variable.ttf", (900, 100, 12, 500), "foreverpin", -0.035),
    ("03  Soft", "Fredoka 700 / foreverpin", SOURCE / "Fredoka-variable.ttf", (700, 100), "foreverpin", -0.035),
)


def font(path: Path, size: int, axes: tuple[int, ...]):
    result = ImageFont.truetype(str(path), size)
    result.set_variation_by_axes(list(axes))
    return result


def lettering(path: Path, axes: tuple[int, ...], value: str, tracking: float, height: int):
    face = font(path, 200, axes)
    layer = Image.new("L", (2400, 360))
    draw = ImageDraw.Draw(layer)
    for index, character in enumerate(value):
        x = 24 + face.getlength(value[:index]) + index * 200 * tracking
        draw.text((round(x), 18), character, font=face, fill=255)
    layer = layer.crop(layer.getbbox())
    return layer.resize((round(layer.width * height / layer.height), height), Image.Resampling.LANCZOS)


def recolor(mask: Image.Image, color: str):
    ink = Image.new("RGBA", mask.size, color)
    ink.putalpha(mask)
    return ink


def fit(image: Image.Image, max_width: int, max_height: int):
    image = image.copy()
    image.thumbnail((max_width, max_height), Image.Resampling.LANCZOS)
    return image


def caption(draw: ImageDraw.ImageDraw, xy, value: str, color="#202331", size=28):
    draw.text(xy, value, fill=color, font=font(GEIST, size, (500,)))


def render():
    board = Image.new("RGB", (2300, 1450), "#f4f5f8")
    draw = ImageDraw.Draw(board)
    caption(draw, (28, 22), "ForeverPin / lettering study", size=38)
    caption(draw, (28, 71), "Same approved Routed QR symbol, palette and symbol-to-name gap in every row.", size=23)

    for x, title, file, width in (
        (24, "WoW2 / approved lettering", WOW2, 540),
        (1176, "Wheelhouse / approved lettering", WHEELHOUSE, 760),
    ):
        draw.rounded_rectangle((x, 122, x + 1100, 298), radius=20, fill="white")
        caption(draw, (x + 24, 138), title, size=22)
        original = Image.open(file).convert("RGBA")
        preview = fit(original, width, 105)
        board.paste(preview, (x + 24, 179 + (105 - preview.height) // 2), preview)

    original_symbol = Image.open(SYMBOL).convert("L")
    mark = original_symbol.resize((round(original_symbol.width * 132 / original_symbol.height), 132), Image.Resampling.LANCZOS)
    for row, (key, title, path, axes, value, tracking) in enumerate(OPTIONS):
        y = 321 + row * 260
        text_mask = lettering(path, axes, value, tracking, 110)
        for x, dark in ((24, False), (1176, True)):
            canvas = "#14151a" if dark else "white"
            foreground = "#f1f3f8" if dark else "#202331"
            accent = "#aa8cff" if dark else "#7040d7"
            draw.rounded_rectangle((x, y, x + 1100, y + 238), radius=20, fill=canvas)
            caption(draw, (x + 24, y + 18), f"{key}  {title}  /  {'dark' if dark else 'light'}", foreground, 22)
            symbol = recolor(mark, accent)
            board.paste(symbol, (x + 30, y + 76), symbol)
            words = recolor(text_mask, foreground)
            logo_x = x + 30 + mark.width + 26
            logo_y = y + 76 + (mark.height - words.height) // 2
            if logo_x + words.width > x + 1082:
                raise ValueError(f"{key} lettering exceeds the review frame")
            board.paste(words, (logo_x, logo_y), words)

    caption(draw, (28, 1380), "Selected 02. Canonical exports and the application header use this lettering.", size=23)
    board.save(HERE / "preview.png")
    print(HERE / "preview.png")


if __name__ == "__main__":
    render()
