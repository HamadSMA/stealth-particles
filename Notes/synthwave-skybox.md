## Procedural Synthwave Skybox

The level is viewed down a pitched-back camera ([[top-down-camera]]), so everything
above the play slab is *sky* — and an empty camera clears to a flat colour, which
reads as a dead black void behind a neon scene. The fix is a backdrop, but a painted
cubemap texture is the wrong tool here: the look wanted (a horizon gradient, a banded
sunset sun, a scatter of stars) is all smooth math, so a **procedural skybox shader**
renders it analytically — no texture memory, resolution-independent, and every part
is a tweakable property instead of a baked pixel.

The whole image is computed per fragment from one input: the *direction* the pixel
looks in. There is no geometry to sample, only a function of a unit vector.

| Backdrop option | Pros | Cons |
| --- | --- | --- |
| Procedural skybox shader | resolution-independent; zero texture memory; every feature is a live property; trivially animatable | all-math, so organic detail is hard; cost is per-fragment, paid every pixel of sky |
| Painted cubemap texture | any art is possible; cheap to sample | fixed resolution; large download (WebGL-sensitive); editing means re-exporting the art |

---
### Shading by view direction

A skybox pass draws a mesh surrounding the camera, and the useful trick is that each
vertex's **object-space position is the direction toward that part of the sky**. Pass
it through unchanged and normalize in the fragment, and you have the per-pixel view
ray to drive everything off of. The vertical component `d.y` is effectively latitude:
`+1` straight up, `0` at the horizon, `-1` straight down.

**In code (`SynthwaveSkybox.shader`):**

```hlsl
v2f vert (appdata v)
{
    v2f o;
    o.pos = UnityObjectToClipPos(v.vertex);
    o.dir = v.vertex.xyz;
    return o;
}

fixed4 frag (v2f i) : SV_Target
{
    float3 d = normalize(i.dir);
    float h = d.y;
```

- `o.dir = v.vertex.xyz` carries the raw object-space vertex out untouched; the interpolator hands the fragment a smoothly-varying direction across the sky.
- `normalize(i.dir)` undoes the interpolation's length distortion so `d` is a true unit direction.
- `h = d.y` is the one scalar the gradient and the horizon glow are built from — height above (or below) the horizon.

---
### The vertical gradient and horizon glow

A sunset sky is darkest overhead and hottest at the horizon, so the base colour is a
height-driven blend between three bands, plus a tight glow strip where the sky meets
the ground to sell the light source sitting just over the edge.

**In code (`SynthwaveSkybox.shader`):**

```hlsl
float a = saturate(h);
float3 col = lerp(_HorizonColor.rgb, _TopColor.rgb, pow(a, _HorizonFalloff));
col = lerp(col, _GroundColor.rgb, saturate(-h * 1.7));
float glow = exp(-abs(h) / _GlowWidth) * _GlowIntensity;
col += _HorizonColor.rgb * glow;
```

- `lerp(_HorizonColor, _TopColor, pow(a, _HorizonFalloff))` runs the gradient from horizon to zenith; raising `a` to `_HorizonFalloff` pushes the transition up high so most of the dome stays dark and the hot colour hugs the horizon.
- `saturate(-h * 1.7)` is non-zero only below the horizon, so the second `lerp` swaps in `_GroundColor` for the lower hemisphere without touching the sky.
- `exp(-abs(h) / _GlowWidth)` is a sharp falloff peaking at `h = 0`; multiplied by `_HorizonColor` it adds a bright line exactly along the horizon, brightest where `_GlowWidth` is small.

---
### The sun: a little perspective projection

The sun is a disk *parked in a direction*, not at a screen position, so it stays put
as the camera turns. The technique is to build a tiny camera looking down `sunDir`:
project the view ray onto a plane facing the sun and read off local 2-D coordinates.
A radius from that origin gives the disk; the vertical coordinate drives both the
top-to-bottom colour gradient and the retro horizontal bands.

**In code (`SynthwaveSkybox.shader`):**

```hlsl
float3 sunDir = normalize(float3(0.0, _SunElevation, 1.0));
float3 right = normalize(cross(float3(0.0, 1.0, 0.0), sunDir));
float3 upv = cross(sunDir, right);
float zc = dot(d, sunDir);

float sunR = 1000.0;
float sunYN = 0.0;
if (zc > 0.0)
{
    float xc = dot(d, right) / zc;
    float yc = dot(d, upv) / zc;
    sunR = sqrt(xc * xc + yc * yc);
    sunYN = saturate((yc + _SunSize) / (2.0 * _SunSize));
}
```

- `right` and `upv` are an orthonormal basis on the plane facing `sunDir`, built with two cross products — the sun's local axes.
- `zc = dot(d, sunDir)` is how much the ray points *at* the sun; the `zc > 0.0` guard skips the whole back hemisphere where the sun can't be.
- Dividing `dot(d, right)` and `dot(d, upv)` by `zc` is the perspective divide: it projects the ray onto the facing plane, giving sun-local `xc`/`yc`. `sunR` is the distance from the sun's centre; `sunYN` remaps the vertical axis to `0..1` across the disk for the gradient and bands.

The disk, its colour gradient, and the bands are then composited:

**In code (`SynthwaveSkybox.shader`):**

```hlsl
float3 sunCol = lerp(_SunBottomColor.rgb, _SunTopColor.rgb, yn);

float band = 1.0;
if (yn < 0.5)
{
    float f = (0.5 - yn) / 0.5;
    float s = frac(yn * _BandCount);
    band = step(f * _BandMaxGap, s);
}

float disk = smoothstep(_SunSize, _SunSize - _SunEdge, r);
float sun = disk * band;
float halo = smoothstep(_SunSize * 2.6, _SunSize, r) * _HaloIntensity;
col += _SunBottomColor.rgb * halo * (1.0 - disk);
col = lerp(col, sunCol, sun);
```

- `disk` is a soft-edged mask: `smoothstep` from `_SunSize` inward to `_SunSize - _SunEdge` gives a filled circle with an anti-aliased rim of width `_SunEdge`.
- `band` carves the signature horizontal slits, but only on the lower half (`yn < 0.5`). `f` grows toward the bottom, so `step(f * _BandMaxGap, s)` widens the gaps the lower you go — the gaps fan out, the way a synthwave sun's stripes do.
- `halo` is a second, larger `smoothstep` out to `2.6 *_SunSize`; adding it as `* (1.0 - disk)` lays a glow around the sun without doubling up over the solid disk.
- `lerp(col, sunCol, sun)` finally stamps the banded disk over the sky.

---
### Stars: hashing a grid into points

Stars are a sparse field of bright dots, generated rather than placed. The sky is
diced into a grid; each cell gets a pseudo-random value from a hash; only cells above
a threshold light up, and within a lit cell a point is drawn by distance to its
centre. A `frac`-based hash is the standard GPU way to fake randomness with no texture
or buffer.

**In code (`SynthwaveSkybox.shader`):**

```hlsl
float2 sc = d.xz / (abs(h) + 0.25);
float2 cell = floor(sc * _StarDensity);
float n = hash21(cell);
float starHit = step(1.0 - _StarAmount * 0.12, n);
float2 sub = frac(sc * _StarDensity) - 0.5;
float pt = starHit * smoothstep(0.34, 0.0, length(sub) * 2.0);
float starFade = saturate(1.0 - glow * 2.5);
```

- `d.xz / (abs(h) + 0.25)` projects the direction to a 2-D star plane; the `abs(h)` divisor spreads cells near the horizon so stars don't pinch to a point overhead.
- `floor(sc * _StarDensity)` is the cell id and `frac(...) - 0.5` is the position within the cell; `hash21` turns the id into a stable per-cell random `n`.
- `step(1.0 - _StarAmount * 0.12, n)` keeps only the rare cells whose `n` clears the threshold — `_StarAmount` is the dial for *how many* cells qualify, and the `0.12` factor sets its usable range.
- `smoothstep(0.34, 0.0, length(sub) * 2.0)` shapes the dot: brightest at the cell centre, fading to nothing at the edge. `starFade` dims stars inside the horizon glow so they don't fight the bright band.

---
### Layering in one pass: masking, not draw order

Everything composites into a single `col` in one fragment, so there is no depth or
draw order to lean on — "behind" and "in front" only exist as the sequence of
arithmetic. That matters the moment two additive elements overlap. The stars are
*added* to `col`, and the sun's halo is additive while its band gaps leave `col`
untouched; so simply computing the sun after the stars does **not** hide them — they
keep glowing through the halo and through the slits, looking like stars stuck on the
sun's face. The cure is to compute a **region mask** for the sun up front and gate the
stars with it, rather than relying on order.

**In code (`SynthwaveSkybox.shader`):**

```hlsl
sunRegion = smoothstep(_SunSize * 2.6, _SunSize, sunR);
...
col += pt * _StarBrightness * (0.5 + n * 0.5) * starFade * (1.0 - sunRegion);
```

- `sunRegion` is `1` across the disk, ramps down through the halo, and is `0` past `2.6 *_SunSize` — the same extent the halo occupies, computed before the stars are added.
- Multiplying the star term by `(1.0 - sunRegion)` zeroes stars over the whole sun footprint and eases them back in beyond the glow, so none survive in the disk, the band gaps, or the halo.
- The general lesson for single-pass shading: when later elements are additive or leave gaps, ordering alone won't occlude — you mask the earlier contribution with the later element's coverage.
