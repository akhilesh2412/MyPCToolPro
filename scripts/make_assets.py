"""Generate all binary assets (app icon + README banner) from code.

The GitHub repo ships this script instead of binary files, so a fresh clone
can regenerate everything:

    pip install pillow
    python scripts/make_assets.py

Outputs:
    src/app.ico        (Windows app icon, multi-size)
    assets/icon.png    (256px icon)
    assets/banner.png  (1280x360 README banner)
"""
import os
from PIL import Image, ImageDraw, ImageFont

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))


def shield(draw, cx, cy, scale=1.0, outer=(37, 99, 235), inner=(59, 130, 246)):
    s = scale
    pts = [(cx, cy - 105*s), (cx + 78*s, cy - 72*s), (cx + 78*s, cy + 5*s), (cx + 78*s, cy + 48*s),
           (cx, cy + 110*s), (cx - 78*s, cy + 48*s), (cx - 78*s, cy + 5*s), (cx - 78*s, cy - 72*s)]
    draw.polygon(pts, fill=outer)
    pts2 = [(cx, cy - 88*s), (cx + 62*s, cy - 60*s), (cx + 62*s, cy + 5*s), (cx + 62*s, cy + 40*s),
            (cx, cy + 92*s), (cx - 62*s, cy + 40*s), (cx - 62*s, cy + 5*s), (cx - 62*s, cy - 60*s)]
    draw.polygon(pts2, fill=inner)
    bolt = [(cx + 14*s, cy - 58*s), (cx - 30*s, cy + 22*s), (cx + 2*s, cy + 22*s),
            (cx - 10*s, cy + 62*s), (cx + 38*s, cy - 18*s), (cx + 4*s, cy - 18*s)]
    draw.polygon(bolt, fill=(255, 255, 255))


def make_icon():
    sizes = [(256, 256), (128, 128), (64, 64), (48, 48), (32, 32), (16, 16)]
    imgs = []
    for w, h in sizes:
        img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
        shield(ImageDraw.Draw(img), w // 2, h // 2, scale=w / 256 * 0.92)
        imgs.append(img)
    os.makedirs(os.path.join(ROOT, "src"), exist_ok=True)
    os.makedirs(os.path.join(ROOT, "assets"), exist_ok=True)
    imgs[0].save(os.path.join(ROOT, "src", "app.ico"), format="ICO",
                 sizes=[(i.width, i.height) for i in imgs], append_images=imgs[1:])
    imgs[0].save(os.path.join(ROOT, "assets", "icon.png"))
    # NSIS installer also uses a copy next to the .nsi script
    imgs[0].save(os.path.join(ROOT, "installer", "app.ico"), format="ICO",
                 sizes=[(i.width, i.height) for i in imgs], append_images=imgs[1:])
    print("icon -> src/app.ico, assets/icon.png, installer/app.ico")


def font(sz, bold=True):
    names = ["arialbd.ttf", "Arial Bold.ttf"] if bold else ["arial.ttf", "Arial.ttf"]
    for base in [r"C:\Windows\Fonts", "/usr/share/fonts/truetype/dejavu", "/usr/share/fonts"]:
        for n in (names if "dejav" not in base else
                  (["DejaVuSans-Bold.ttf"] if bold else ["DejaVuSans.ttf"])):
            p = os.path.join(base, n)
            if os.path.exists(p):
                return ImageFont.truetype(p, sz)
    return ImageFont.load_default()


def make_banner():
    W, H = 1280, 360
    img = Image.new("RGB", (W, H), (11, 18, 32))
    d = ImageDraw.Draw(img)
    for cx, cy, r, col in [(1120, 60, 180, (30, 41, 59)), (120, 300, 140, (30, 41, 59)), (640, 180, 420, (15, 23, 42))]:
        d.ellipse([cx - r, cy - r, cx + r, cy + r], fill=col)
    d.rectangle([0, H - 6, W, H], fill=(59, 130, 246))
    shield(d, 180, 175)
    d.text((300, 105), "MyPC Tool Pro", font=font(72), fill=(241, 245, 249))
    d.text((302, 195), "Cleaner  -  Optimizer  -  Process Killer  -  RAM Booster",
           font=font(28, False), fill=(148, 163, 184))
    d.rounded_rectangle([302, 262, 560, 308], radius=23, fill=(225, 29, 72))
    d.text((326, 270), "KILL ALL PROCESSES", font=font(24), fill=(255, 255, 255))
    d.rounded_rectangle([578, 262, 760, 308], radius=23, outline=(59, 130, 246), width=2)
    d.text((612, 270), "v2.0  -  WINDOWS", font=font(24), fill=(96, 165, 250))
    os.makedirs(os.path.join(ROOT, "assets"), exist_ok=True)
    img.save(os.path.join(ROOT, "assets", "banner.png"))
    print("banner -> assets/banner.png")


if __name__ == "__main__":
    make_icon()
    make_banner()
