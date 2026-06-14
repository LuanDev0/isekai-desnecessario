// Gera favicon.ico (16/32/48/64) com o mark do Isekai Desnecessário.
// Sem dependências externas — usa só 'zlib' e 'fs' do Node.
const fs = require('fs');
const zlib = require('zlib');

// ── Paleta ─────────────────────────────────────────────
const BG     = [13, 17, 23];     // #0d1117
const BORDER = [48, 54, 61];     // #30363d
const BLUE   = [88, 166, 255];   // #58a6ff
const PURPLE = [188, 140, 255];  // #bc8cff

// ── Helpers de geometria ───────────────────────────────
function clamp(v, a, b) { return v < a ? a : v > b ? b : v; }
function smooth(edge0, edge1, x) {
  const t = clamp((x - edge0) / (edge1 - edge0), 0, 1);
  return t * t * (3 - 2 * t);
}
// distância de P até segmento AB
function distSeg(px, py, ax, ay, bx, by) {
  const dx = bx - ax, dy = by - ay;
  const l2 = dx * dx + dy * dy;
  let t = l2 === 0 ? 0 : ((px - ax) * dx + (py - ay) * dy) / l2;
  t = clamp(t, 0, 1);
  const cx = ax + t * dx, cy = ay + t * dy;
  return Math.hypot(px - cx, py - cy);
}
// distância até polilinha (chevron = 2 segmentos)
function distPoly(px, py, pts) {
  let d = Infinity;
  for (let i = 0; i < pts.length - 1; i++)
    d = Math.min(d, distSeg(px, py, pts[i][0], pts[i][1], pts[i + 1][0], pts[i + 1][1]));
  return d;
}
// distância assinada de um retângulo arredondado centrado
function sdRoundRect(px, py, halfW, halfH, r) {
  const qx = Math.abs(px) - (halfW - r);
  const qy = Math.abs(py) - (halfH - r);
  const ox = Math.max(qx, 0), oy = Math.max(qy, 0);
  return Math.hypot(ox, oy) + Math.min(Math.max(qx, qy), 0) - r;
}

function over(dst, src, a) { // alpha-blend src sobre dst
  return [
    Math.round(src[0] * a + dst[0] * (1 - a)),
    Math.round(src[1] * a + dst[1] * (1 - a)),
    Math.round(src[2] * a + dst[2] * (1 - a)),
  ];
}

// ── Render de um tamanho (com supersampling 4x) ────────
function render(size) {
  const SS = 4;
  const N = size * SS;
  const px = (i) => (i + 0.5) / N; // 0..1

  // chevron em coords normalizadas
  const chev = [[0.22, 0.615], [0.50, 0.345], [0.78, 0.615]];
  const chevHalf = 0.092;      // metade da espessura
  const edge = 1.1 / N;        // suavidade da borda (em unidades normalizadas)

  const out = Buffer.alloc(size * size * 4);

  for (let y = 0; y < size; y++) {
    for (let x = 0; x < size; x++) {
      let r = 0, g = 0, b = 0, a = 0;
      for (let sy = 0; sy < SS; sy++) {
        for (let sx = 0; sx < SS; sx++) {
          const nx = px(x * SS + sx);
          const ny = px(y * SS + sy);

          // fundo: retângulo arredondado centrado em (0.5,0.5)
          const sd = sdRoundRect(nx - 0.5, ny - 0.5, 0.5, 0.5, 0.18);
          let bgA = smooth(edge, -edge, sd); // dentro -> 1
          if (bgA <= 0) continue;            // fora do ícone = transparente

          let col = BG.slice();

          // anel de borda sutil
          const ring = smooth(edge, -edge, Math.abs(sd + 0.035) - 0.02);
          col = over(col, BORDER, ring * 0.55);

          // chevron azul
          const dc = distPoly(nx, ny, chev);
          const chevA = smooth(chevHalf + edge, chevHalf - edge, dc);
          col = over(col, BLUE, chevA);

          // ponto/acento roxo embaixo do chevron (suaviza no 16px vira brilho)
          const dp = Math.hypot(nx - 0.5, ny - 0.74);
          const dotA = smooth(0.052 + edge, 0.052 - edge, dp);
          col = over(col, PURPLE, dotA * 0.9);

          r += col[0] * bgA; g += col[1] * bgA; b += col[2] * bgA; a += bgA;
        }
      }
      const n = SS * SS;
      const idx = (y * size + x) * 4;
      const alpha = a / n;
      if (alpha > 0) {
        out[idx]     = Math.round(r / a);
        out[idx + 1] = Math.round(g / a);
        out[idx + 2] = Math.round(b / a);
      }
      out[idx + 3] = Math.round(alpha * 255);
    }
  }
  return out; // RGBA
}

// ── Encoder PNG mínimo ─────────────────────────────────
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
  const len = Buffer.alloc(4);
  len.writeUInt32BE(data.length, 0);
  const t = Buffer.from(type, 'ascii');
  const crc = Buffer.alloc(4);
  crc.writeUInt32BE(crc32(Buffer.concat([t, data])), 0);
  return Buffer.concat([len, t, data, crc]);
}
function encodePNG(rgba, size) {
  const sig = Buffer.from([137, 80, 78, 71, 13, 10, 26, 10]);
  const ihdr = Buffer.alloc(13);
  ihdr.writeUInt32BE(size, 0);
  ihdr.writeUInt32BE(size, 4);
  ihdr[8] = 8;   // bit depth
  ihdr[9] = 6;   // RGBA
  // raw com filtro 0 por linha
  const raw = Buffer.alloc((size * 4 + 1) * size);
  for (let y = 0; y < size; y++) {
    raw[y * (size * 4 + 1)] = 0;
    rgba.copy(raw, y * (size * 4 + 1) + 1, y * size * 4, (y + 1) * size * 4);
  }
  const idat = zlib.deflateSync(raw, { level: 9 });
  return Buffer.concat([sig, chunk('IHDR', ihdr), chunk('IDAT', idat), chunk('IEND', Buffer.alloc(0))]);
}

// ── Empacota .ico (entradas PNG) ───────────────────────
function buildIco(sizes) {
  const pngs = sizes.map((s) => encodePNG(render(s), s));
  const header = Buffer.alloc(6);
  header.writeUInt16LE(0, 0);          // reserved
  header.writeUInt16LE(1, 2);          // type = icon
  header.writeUInt16LE(sizes.length, 4);
  const entries = [];
  let offset = 6 + sizes.length * 16;
  sizes.forEach((s, i) => {
    const e = Buffer.alloc(16);
    e[0] = s >= 256 ? 0 : s;           // width
    e[1] = s >= 256 ? 0 : s;           // height
    e[2] = 0;                          // palette
    e[3] = 0;                          // reserved
    e.writeUInt16LE(1, 4);             // planes
    e.writeUInt16LE(32, 6);            // bpp
    e.writeUInt32LE(pngs[i].length, 8);
    e.writeUInt32LE(offset, 12);
    offset += pngs[i].length;
    entries.push(e);
  });
  return Buffer.concat([header, ...entries, ...pngs]);
}

const ico = buildIco([16, 32, 48, 64]);
fs.writeFileSync('public/favicon.ico', ico);
// PNG grande extra para uso geral (lojas, redes)
fs.writeFileSync('public/icon-256.png', encodePNG(render(256), 256));
console.log('favicon.ico gerado:', ico.length, 'bytes');
console.log('icon-256.png gerado');
