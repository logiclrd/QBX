WEBRAY
======

A texture-mapped SCREEN 7 raycaster written for WebQB and QuickBASIC. Its
320-by-120 world view and 80-pixel dashboard follow Catacomb Abyss's original
EGA screen proportions.

Controls
--------

Up / W       Walk forward
Down / S     Walk backward
Left / Right Turn
A / D        Strafe
Space        Fire the crystal wand
Esc          Leave the dungeon

The renderer casts 160 depth-tested rays into alternating EGA video pages.
Its 40-by-28 starting level is the original Towne Cemetary map from the
shareware release. All 12 wall codes in that level use their proper 64-by-64
east/west and north/south artwork, decoded directly from EGAGRAPH.ABS.

The map info plane now supplies the original enemy and item placements. The
level contains animated zombies and bats, spell bolts, potions, colored keys,
a gem, a scroll, treasure chests, tombstones, destructible walls, keyed doors,
and the locked exit. Enemy, pickup, scenery, and projectile art is also decoded
directly from the shareware EGAGRAPH data. Walk into pickups to collect them,
walk into a keyed gate to open it, and shoot cracked wall sections.
Keyboard input is drained once per frame so slow machines do not accumulate
stale moves.

The packed CATSPR.BSV sprite bank and CATPACK.BSV wall bank each remain below
QuickBASIC's per-array segment limit while keeping every EGA pixel directly
addressable by the renderer. WEBRAY.BAS and WEBRAYHQ.BAS currently use the
same portable renderer.

The status panel, five damage faces, radar, and gem indicators are the original
Catacomb Abyss graphics decoded from EGAGRAPH.ABS. Health, inventory, keys,
nearby objects, and collected treasure update while the game runs.
