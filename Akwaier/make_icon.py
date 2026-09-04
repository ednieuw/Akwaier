"""
Maakt akwaier.ico: een tegel met de letter A, in de kleur van keten A.
Puur standaardbibliotheek; er is geen tekenpakket voor nodig.
"""
import struct

SS = 192                                  # deelbaar door 48, 32 en 16
BG = (69, 90, 100)                        # #455a64, keten A in de moderne skin
FG = (255, 255, 255)


def dist_to_segment(px, py, ax, ay, bx, by):
    dx, dy = bx - ax, by - ay
    t = 0.0 if dx == dy == 0 else ((px - ax) * dx + (py - ay) * dy) / (dx * dx + dy * dy)
    t = max(0.0, min(1.0, t))
    return ((px - (ax + t * dx)) ** 2 + (py - (ay + t * dy)) ** 2) ** 0.5


STROKES = [((.30, .80), (.50, .20)),      # linkerpoot van de A
           ((.70, .80), (.50, .20)),      # rechterpoot
           ((.375, .60), (.625, .60))]    # dwarsbalk
THICK = .055
RADIUS = .16                              # afronding van de hoeken


def render(n):
    """Levert n x n pixels als (r, g, b, a), rij voor rij van boven naar beneden."""
    out = []
    for py in range(n):
        for px in range(n):
            u, v = (px + .5) / n, (py + .5) / n
            # afgeronde rechthoek als achtergrond
            cx = min(max(u, RADIUS), 1 - RADIUS)
            cy = min(max(v, RADIUS), 1 - RADIUS)
            d = ((u - cx) ** 2 + (v - cy) ** 2) ** 0.5
            alpha = 255 if d <= RADIUS else 0
            col = BG
            if alpha:
                for (ax, ay), (bx, by) in STROKES:
                    if dist_to_segment(u, v, ax, ay, bx, by) <= THICK:
                        col = FG
                        break
            out.append((col[0], col[1], col[2], alpha))
    return out


def downsample(src, n, k):
    """k x k pixels middelen tot een beeld van n // k breed."""
    m = n // k
    dst = []
    for y in range(m):
        for x in range(m):
            r = g = b = a = 0
            for j in range(k):
                for i in range(k):
                    pr, pg, pb, pa = src[(y * k + j) * n + (x * k + i)]
                    r += pr * pa; g += pg * pa; b += pb * pa; a += pa
            if a:
                dst.append((r // a, g // a, b // a, a // (k * k)))
            else:
                dst.append((0, 0, 0, 0))
    return dst


def bmp_image(pix, n):
    """BITMAPINFOHEADER + BGRA van onder naar boven + lege AND-masker."""
    hdr = struct.pack("<IiiHHIIiiII", 40, n, n * 2, 1, 32, 0, n * n * 4, 0, 0, 0, 0)
    body = bytearray()
    for y in range(n - 1, -1, -1):
        for x in range(n):
            r, g, b, a = pix[y * n + x]
            body += bytes((b, g, r, a))
    rowbytes = ((n + 31) // 32) * 4
    return hdr + bytes(body) + bytes(rowbytes * n)


def main(path="akwaier.ico", sizes=(16, 32, 48)):
    big = render(SS)
    images = [bmp_image(downsample(big, SS, SS // s), s) for s in sizes]
    out = struct.pack("<HHH", 0, 1, len(sizes))
    offset = 6 + 16 * len(sizes)
    for s, img in zip(sizes, images):
        out += struct.pack("<BBBBHHII", s % 256, s % 256, 0, 0, 1, 32, len(img), offset)
        offset += len(img)
    with open(path, "wb") as fh:
        fh.write(out + b"".join(images))
    print(f"{path} geschreven: {', '.join(f'{s}x{s}' for s in sizes)}")


if __name__ == "__main__":
    main()
