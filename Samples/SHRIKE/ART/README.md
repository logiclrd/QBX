# General Info

All of these sprites required considerable editing and tweaking. Universally, the sources weren't actually 1-to-1 pixels, and everything was 8:8:8 RGB and needed to be palettized.

GIMP and Pixelorama were used to manipulate images. The Convert project in this directory translates PNGs that have been mapped onto the VGA palette to ART files, a format specific to SHRIKE.

## ART File Format

This is a very simple file format.

* Header (offset `0`):

    * `byte width`
    * `byte height`

* Pixels (offset `2`):

    * `byte[] data`, length `width * height`

* Mask (offset `2 + data.Length`):

    * `byte[] mask`, length `width * height`

A pixel is transparent if the corresponding mask byte is 0, otherwise it is fully-opaque.

## Sources

### ALIEN0

Clanker-generated.

### ALIEN1

Clanker-generated.

### ALIEN2

Clanker-generated.

### ALIEN3

Clanker-generated.

### ALIEN4

Clanker-generated.

### MINE

Clanker-generated.

### SPACEMAN

Clanker-generated.

### DOG

Clanker-generated.

### BOOM

Source: https://www.vecteezy.com/vector-art/23530772-pixel-art-explosion-boom-or-burst-animate-sprites

### ROCK

Source: https://www.reddit.com/r/PixelArt/comments/cxn5aq/attempt_at_rotating_pixel_art_asteroid_while/

