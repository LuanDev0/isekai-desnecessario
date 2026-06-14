// Gera favicon.ico (multi-tamanho) + icon-256.png para o Isekai Desnecessário.
// Desenha um portal em arco pontiagudo com a silhueta de um herói dentro.
// Sem dependências externas: rasteriza por supersampling, encoda PNG via zlib nativo
// e empacota os PNGs no formato ICO.
import { deflateSync } from 'node:zlib';
import { writeFileSync, mkdirSync } from 'node:fs';
import { dirname, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';

const __dirname = dirname(fileURLToPath(import.meta.url));
const OUT_DIR = resolve(__dirname, '..', 'public');

// ── Paleta ───────────────────────────────────────────────
const BG       = [0x0d, 0x11, 0x17];          // fundo escuro
const GLOW     = [0x16, 0x26, 0x3f];          // brilho interno do portal
const BLUE_TOP = [0x79, 0xc0, 0xff];          // topo do gradiente (arco)
const PURP_BOT = [0xc9, 0x9c, 0xff];          // base do gradiente (arco)
const FIG_TOP  = [0x8e, 0xc3, 0xff];          // figura — topo
const FIG_BOT  = [0xc9, 0xa9, 0xff];          // figura — base
const SPARK    = [0xa9, 0xd3, 0xff];          // brilhos

const lerp = (a, b, t) => a + (b - a) * t;
const mix  = (c1, c2, t) => [lerp(c1[0], c2[0], t), lerp(c1[1], c2[1], t), lerp(c1[2], c2[2], t)];
const clamp01 = v => (v < 0 ? 0 : v > 1 ? 1 : v);

// Gradiente vertical reaproveitado pelo arco e pela figura
function gradient(y, top, bot, y0 = 0.07, y1 = 0.82) {
  return mix(top, bot, clamp01((y - y0) / (y1 - y0)));
}

// ── Geometria do portal (coords normalizadas 0..1, y para baixo) ──
const xL = 0.30, xR = 0.70;     // pernas do arco
const yBase = 0.82;             // base (chão)
const ySh = 0.42;               // linha de nascença do arco
const S = xR - xL;              // vão
const STROKE = 0.068;           // espessura do contorno

// Nível de detalhe — brilhos só nos tamanhos maiores (no favicon viram ruído)
let DETAIL = true;

// Está dentro do "vão" sólido da porta (retângulo + arco pontiagudo)?
function insideDoorway(x, y, l, r, sh, base) {
  if (x < l || x > r || y > base) return false;
  if (y >= sh) return true;                       // parte reta (pernas)
  const span = r - l;
  const dL = Math.hypot(x - l, y - sh);           // arco lanceta: 2 arcos
  const dR = Math.hypot(x - r, y - sh);
  return dL <= span && dR <= span;
}

// Silhueta do herói dentro do portal
function insideFigure(x, y) {
  const dx = x - 0.5;
  // cabeça
  if (Math.hypot(dx, y - 0.50) <= 0.050) return true;
  // manto: trapézio que alarga até a base
  if (y >= 0.545 && y <= 0.80) {
    const hw = lerp(0.052, 0.115, (y - 0.545) / (0.80 - 0.545));
    if (Math.abs(dx) <= hw) return true;
  }
  return false;
}

// Brilhos laterais (pares espelhados)
const SPARKS = [
  [0.16, 0.30, 0.016], [0.16, 0.46, 0.012], [0.18, 0.62, 0.013],
  [0.84, 0.30, 0.016], [0.84, 0.46, 0.012], [0.82, 0.62, 0.013],
];
function sparkAlpha(x, y) {
  let a = 0;
  for (const [sx, sy, sr] of SPARKS) {
    const d = Math.hypot(x - sx, y - sy);
    if (d <= sr) a = Math.max(a, 1 - d / sr);
  }
  return a * 0.9;
}

// Fundo arredondado (quadrado com cantos arredondados, cantos transparentes)
function insideRounded(x, y, r = 0.18) {
  const ax = Math.abs(x - 0.5), ay = Math.abs(y - 0.5);
  if (ax > 0.5 || ay > 0.5) return false;
  if (ax <= 0.5 - r || ay <= 0.5 - r) return true;
  const cx = ax - (0.5 - r), cy = ay - (0.5 - r);
  return cx * cx + cy * cy <= r * r;
}

// "over" composite (alpha reto)
function over(dst, src) {
  const sa = src[3];
  if (sa <= 0) return dst;
  const da = dst[3];
  const oa = sa + da * (1 - sa);
  if (oa <= 0) return [0, 0, 0, 0];
  const r = (src[0] * sa + dst[0] * da * (1 - sa)) / oa;
  const g = (src[1] * sa + dst[1] * da * (1 - sa)) / oa;
  const b = (src[2] * sa + dst[2] * da * (1 - sa)) / oa;
  return [r, g, b, oa];
}

// Cor final (alpha reto, canais 0..255) de um ponto normalizado
function sample(x, y) {
  let px = [0, 0, 0, 0];
  if (!insideRounded(x, y)) return px;

  px = over(px, [...BG, 1]);

  // brilho interno do portal (área aberta)
  const innerOpen = insideDoorway(x, y, xL + STROKE, xR - STROKE, ySh, yBase + 0.3);
  if (innerOpen) {
    const t = clamp01((y - ySh) / (yBase - ySh));
    px = over(px, [...GLOW, 0.55 + 0.30 * t]);
  }

  // brilhos (apenas em tamanhos maiores)
  if (DETAIL) {
    const sa = sparkAlpha(x, y);
    if (sa > 0) px = over(px, [...SPARK, sa]);
  }

  // figura
  if (insideFigure(x, y)) px = over(px, [...gradient(y, FIG_TOP, FIG_BOT, 0.47, 0.80), 1]);

  // contorno do arco (porta sólida externa menos interna; base aberta)
  const outer = insideDoorway(x, y, xL, xR, ySh, yBase);
  const inner = insideDoorway(x, y, xL + STROKE, xR - STROKE, ySh, yBase + 0.3);
  if (outer && !inner) px = over(px, [...gradient(y, BLUE_TOP, PURP_BOT), 1]);

  return px;
}

// ── Rasteriza um tamanho com supersampling ───────────────
function renderRGBA(size, SS = 4) {
  DETAIL = size >= 64;
  const buf = Buffer.alloc(size * size * 4);
  const inv = 1 / size;
  const subInv = 1 / SS;
  for (let py = 0; py < size; py++) {
    for (let px = 0; px < size; px++) {
      let r = 0, g = 0, b = 0, a = 0;
      for (let sy = 0; sy < SS; sy++) {
        for (let sx = 0; sx < SS; sx++) {
          const nx = (px + (sx + 0.5) * subInv) * inv;
          const ny = (py + (sy + 0.5) * subInv) * inv;
          const c = sample(nx, ny);
          // acumula em premultiplicado
          r += c[0] * c[3]; g += c[1] * c[3]; b += c[2] * c[3]; a += c[3];
        }
      }
      const n = SS * SS;
      const oa = a / n;
      const o = (py * size + px) * 4;
      const to8 = v => Math.max(0, Math.min(255, Math.round(v)));
      if (oa > 0) {
        buf[o]     = to8((r / n) / oa);
        buf[o + 1] = to8((g / n) / oa);
        buf[o + 2] = to8((b / n) / oa);
      }
      buf[o + 3] = to8(oa * 255);
    }
  }
  return buf;
}

// ── Encoder PNG (RGBA, sem filtro) ───────────────────────
const CRC_TABLE = (() => {
  const t = new Uint32Array(256);
  for (let n = 0; n < 256; n++) {
    let c = n;
    for (let k = 0; k < 8; k++) c = c & 1 ? 0xedb88320 ^ (c >>> 1) : c >>> 1;
    t[n] = c >>> 0;
  }
  return t;
})();
function crc32(buf) {
  let c = 0xffffffff;
  for (let i = 0; i < buf.length; i++) c = CRC_TABLE[(c ^ buf[i]) & 0xff] ^ (c >>> 8);
  return (c ^ 0xffffffff) >>> 0;
}
function chunk(type, data) {
  const len = Buffer.alloc(4); len.writeUInt32BE(data.length, 0);
  const typeBuf = Buffer.from(type, 'ascii');
  const crcBuf = Buffer.alloc(4); crcBuf.writeUInt32BE(crc32(Buffer.concat([typeBuf, data])), 0);
  return Buffer.concat([len, typeBuf, data, crcBuf]);
}
function encodePNG(rgba, size) {
  const sig = Buffer.from([0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a]);
  const ihdr = Buffer.alloc(13);
  ihdr.writeUInt32BE(size, 0); ihdr.writeUInt32BE(size, 4);
  ihdr[8] = 8; ihdr[9] = 6; ihdr[10] = 0; ihdr[11] = 0; ihdr[12] = 0;
  const raw = Buffer.alloc((size * 4 + 1) * size);
  for (let y = 0; y < size; y++) {
    raw[y * (size * 4 + 1)] = 0;
    rgba.copy(raw, y * (size * 4 + 1) + 1, y * size * 4, (y + 1) * size * 4);
  }
  const idat = deflateSync(raw, { level: 9 });
  return Buffer.concat([sig, chunk('IHDR', ihdr), chunk('IDAT', idat), chunk('IEND', Buffer.alloc(0))]);
}

// ── Empacotador ICO (entradas PNG) ───────────────────────
function buildICO(entries) {
  const count = entries.length;
  const header = Buffer.alloc(6);
  header.writeUInt16LE(0, 0); header.writeUInt16LE(1, 2); header.writeUInt16LE(count, 4);
  const dir = Buffer.alloc(16 * count);
  let offset = 6 + 16 * count;
  entries.forEach((e, i) => {
    const o = i * 16;
    dir[o] = e.size >= 256 ? 0 : e.size;
    dir[o + 1] = e.size >= 256 ? 0 : e.size;
    dir[o + 2] = 0; dir[o + 3] = 0;
    dir.writeUInt16LE(1, o + 4);
    dir.writeUInt16LE(32, o + 6);
    dir.writeUInt32LE(e.png.length, o + 8);
    dir.writeUInt32LE(offset, o + 12);
    offset += e.png.length;
  });
  return Buffer.concat([header, dir, ...entries.map(e => e.png)]);
}

export { renderRGBA, encodePNG };

// ── Geração ──────────────────────────────────────────────
if (resolve(process.argv[1] || '') === resolve(fileURLToPath(import.meta.url))) {
  mkdirSync(OUT_DIR, { recursive: true });
  const SIZES = [16, 32, 48, 64, 128, 256];
  console.log('Rasterizando:', SIZES.join(', '));
  const entries = SIZES.map(size => {
    const rgba = renderRGBA(size, size <= 48 ? 6 : 4);
    return { size, png: encodePNG(rgba, size) };
  });

  const ico = buildICO(entries);
  writeFileSync(resolve(OUT_DIR, 'favicon.ico'), ico);
  console.log('favicon.ico:', ico.length, 'bytes ->', resolve(OUT_DIR, 'favicon.ico'));

  const png256 = entries.find(e => e.size === 256).png;
  writeFileSync(resolve(OUT_DIR, 'icon-256.png'), png256);
  console.log('icon-256.png:', png256.length, 'bytes');
}
