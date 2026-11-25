#!/usr/bin/env python3
"""
Draw numbered callout markers on Roost screenshots for the in-app guide.
English uses the normal layout; Hebrew mirrors X (the app is RTL), so the
same callouts land on the mirrored controls.
"""
from PIL import Image, ImageDraw, ImageFont
import os

SRC = "/tmp/roost-v11"
OUT = os.path.join(os.path.dirname(__file__), "..", "src", "Roost", "Assets", "help")
os.makedirs(OUT, exist_ok=True)

W = 1060
ACCENT = (46, 196, 184, 255)

# page -> source basename and callout list [(num, x, y), ...] in EN coordinates
PAGES = {
    "dashboard": ("0_dashboard", [(1, 116, 210), (2, 321, 219), (3, 980, 300), (4, 645, 360), (5, 62, 683)]),
    "engines":   ("3_engines",   [(1, 381, 128), (2, 980, 208), (3, 645, 262), (4, 520, 418), (5, 967, 540)]),
    "install":   ("1_install",   [(1, 645, 111), (2, 645, 141)]),
    "apps":      ("2_apps",      [(1, 600, 85), (2, 996, 85)]),
    "linux":     ("4_linux",     [(1, 600, 135), (2, 610, 236), (3, 984, 236), (4, 840, 362), (5, 941, 362)]),
    "settings":  ("5_settings",  [(1, 386, 124), (2, 386, 190), (3, 386, 257), (4, 287, 350), (5, 551, 489), (6, 285, 651)]),
}


def font(size):
    for p in ("/usr/share/fonts/truetype/dejavu/DejaVuSans-Bold.ttf",
              "/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf"):
        if os.path.exists(p):
            return ImageFont.truetype(p, size)
    return ImageFont.load_default()


def marker(draw, x, y, num):
    r = 18
    # white halo for contrast on any background
    draw.ellipse([x - r - 3, y - r - 3, x + r + 3, y + r + 3], fill=(255, 255, 255, 255))
    draw.ellipse([x - r, y - r, x + r, y + r], fill=ACCENT)
    f = font(22)
    t = str(num)
    bb = draw.textbbox((0, 0), t, font=f)
    tw, th = bb[2] - bb[0], bb[3] - bb[1]
    draw.text((x - tw / 2 - bb[0], y - th / 2 - bb[1]), t, font=f, fill=(6, 35, 31, 255))


def main():
    for page, (base, calls) in PAGES.items():
        for lang in ("en", "he"):
            src = os.path.join(SRC, f"{base}_{lang}.png")
            if not os.path.exists(src):
                print("skip (missing):", src); continue
            img = Image.open(src).convert("RGBA")
            d = ImageDraw.Draw(img)
            for num, x, y in calls:
                xx = x if lang == "en" else (W - x)
                marker(d, xx, y, num)
            dst = os.path.join(OUT, f"{page}_{lang}.png")
            img.convert("RGB").save(dst)
            print("wrote", dst)


if __name__ == "__main__":
    main()
