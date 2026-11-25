#!/usr/bin/env python3
"""
Roost icon generator.

A professional, glossy 3D badge: a rounded app-tile descending into a
curved cradle (a "roost") with a soft down-chevron -- i.e. an app coming
to land natively on your system. Platform-neutral so it also fits future
iOS / macOS support.

Outputs a multi-resolution .ico plus PNG previews (light use in README /
in-app logo).
"""
from PIL import Image, ImageDraw, ImageFilter
import os, math

OUT = os.path.join(os.path.dirname(__file__), "..", "src", "Roost", "Assets")
os.makedirs(OUT, exist_ok=True)

SS = 1024
ICO_SIZES = [16, 20, 24, 32, 40, 48, 64, 128, 256]


def lerp(a, b, t):
    return tuple(int(round(a[i] + (b[i] - a[i]) * t)) for i in range(len(a)))


def vgrad(size, top, bottom, mask):
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    px = img.load()
    for y in range(size):
        c = lerp(top, bottom, y / (size - 1))
        for x in range(size):
            px[x, y] = (c[0], c[1], c[2], 255)
    img.putalpha(mask)
    return img


def rounded_mask(size, radius):
    m = Image.new("L", (size, size), 0)
    ImageDraw.Draw(m).rounded_rectangle([0, 0, size - 1, size - 1], radius=radius, fill=255)
    return m


def make_badge():
    S = SS
    img = Image.new("RGBA", (S, S), (0, 0, 0, 0))
    pad = int(S * 0.06)
    inner = S - pad * 2
    radius = int(inner * 0.28)

    # drop shadow
    sh = Image.new("RGBA", (S, S), (0, 0, 0, 0))
    ImageDraw.Draw(sh).rounded_rectangle(
        [pad, pad + int(S * 0.03), pad + inner, pad + inner + int(S * 0.03)],
        radius=radius, fill=(0, 0, 0, 120))
    img = Image.alpha_composite(img, sh.filter(ImageFilter.GaussianBlur(S * 0.035)))

    # body gradient (teal -> indigo)
    top, bottom = (28, 196, 184), (54, 66, 196)
    body = vgrad(inner, top, bottom, rounded_mask(inner, radius))
    layer = Image.new("RGBA", (S, S), (0, 0, 0, 0))
    layer.paste(body, (pad, pad), body)
    img = Image.alpha_composite(img, layer)

    d = ImageDraw.Draw(img)
    d.rounded_rectangle([pad, pad, pad + inner, pad + inner], radius=radius,
                        outline=(255, 255, 255, 46), width=max(2, int(S * 0.006)))

    # glossy top highlight
    gloss = Image.new("RGBA", (S, S), (0, 0, 0, 0))
    ImageDraw.Draw(gloss).rounded_rectangle(
        [pad + int(inner * 0.06), pad + int(inner * 0.05),
         pad + inner - int(inner * 0.06), pad + int(inner * 0.46)],
        radius=int(radius * 0.8), fill=(255, 255, 255, 60))
    gloss = gloss.filter(ImageFilter.GaussianBlur(S * 0.012))
    gm = rounded_mask(S, radius)  # clip to full canvas rounded? use badge mask
    bm = Image.new("L", (S, S), 0)
    ImageDraw.Draw(bm).rounded_rectangle([pad, pad, pad + inner, pad + inner], radius=radius, fill=255)
    gloss.putalpha(Image.composite(gloss.getchannel("A"), Image.new("L", (S, S), 0), bm))
    img = Image.alpha_composite(img, gloss)
    d = ImageDraw.Draw(img)

    cx = S // 2

    # --- the cradle / roost: an upward arc near the lower third ---
    arc_y = int(S * 0.66)
    arc_w = int(inner * 0.52)
    lw = max(8, int(S * 0.030))
    bbox = [cx - arc_w // 2, arc_y - int(arc_w * 0.33),
            cx + arc_w // 2, arc_y + int(arc_w * 0.55)]
    # soft shadow under arc
    sh2 = Image.new("RGBA", (S, S), (0, 0, 0, 0))
    ImageDraw.Draw(sh2).arc(bbox, start=20, end=160, fill=(0, 0, 0, 90), width=lw)
    img = Image.alpha_composite(img, sh2.filter(ImageFilter.GaussianBlur(S * 0.01)))
    d = ImageDraw.Draw(img)
    d.arc(bbox, start=20, end=160, fill=(255, 255, 255, 240), width=lw)

    # --- the app tile descending into the cradle ---
    tile = int(inner * 0.34)
    tx0, ty0 = cx - tile // 2, int(S * 0.30)
    tr = int(tile * 0.26)
    # tile shadow
    ts = Image.new("RGBA", (S, S), (0, 0, 0, 0))
    ImageDraw.Draw(ts).rounded_rectangle([tx0, ty0 + int(S*0.015), tx0 + tile, ty0 + tile + int(S*0.015)],
                                         radius=tr, fill=(0, 0, 0, 110))
    img = Image.alpha_composite(img, ts.filter(ImageFilter.GaussianBlur(S * 0.02)))
    d = ImageDraw.Draw(img)
    # tile body (white with faint vertical gradient)
    tmask = Image.new("L", (S, S), 0)
    ImageDraw.Draw(tmask).rounded_rectangle([tx0, ty0, tx0 + tile, ty0 + tile], radius=tr, fill=255)
    tgrad = Image.new("RGBA", (S, S), (0, 0, 0, 0))
    pxx = tgrad.load()
    for y in range(ty0, ty0 + tile):
        c = lerp((255, 255, 255), (223, 232, 245), (y - ty0) / max(1, tile))
        for x in range(tx0, tx0 + tile):
            pxx[x, y] = (c[0], c[1], c[2], 255)
    tgrad.putalpha(tmask)
    img = Image.alpha_composite(img, tgrad)
    d = ImageDraw.Draw(img)
    # a small down-chevron on the tile (install / landing)
    ccx, ccy = cx, ty0 + int(tile * 0.52)
    cwid = int(tile * 0.30)
    cv = max(6, int(S * 0.018))
    col = (54, 66, 196, 255)
    d.line([(ccx - cwid, ccy - int(cwid * 0.5)), (ccx, ccy + int(cwid * 0.55))], fill=col, width=cv, joint="curve")
    d.line([(ccx + cwid, ccy - int(cwid * 0.5)), (ccx, ccy + int(cwid * 0.55))], fill=col, width=cv, joint="curve")

    return img


def main():
    big = make_badge()
    big.resize((256, 256), Image.LANCZOS).save(os.path.join(OUT, "roost_256.png"))
    big.resize((512, 512), Image.LANCZOS).save(os.path.join(OUT, "roost_512.png"))
    master = big.resize((256, 256), Image.LANCZOS)
    master.save(os.path.join(OUT, "roost.ico"), format="ICO", sizes=[(s, s) for s in ICO_SIZES])
    print("wrote icons to", os.path.abspath(OUT))


if __name__ == "__main__":
    main()
