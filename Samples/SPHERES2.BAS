TYPE Vector
  X AS SINGLE
  Y AS SINGLE
  Z AS SINGLE
END TYPE

TYPE LightSource
  Location AS Vector
  Intensity AS INTEGER
END TYPE

DECLARE SUB DrawSphere (centreX%, centreY%, r%, c%)
DECLARE SUB SetPalette ()
DECLARE FUNCTION VecDot! (a AS Vector, b AS Vector)
DECLARE SUB VecSub (a AS Vector, b AS Vector)
DECLARE SUB VecMul (a AS Vector, m AS SINGLE)

DIM SHARED Lights(0 TO 1) AS LightSource
DIM SHARED AmbientLight!, Shininess!
DIM SHARED CameraZ!

Lights(0).Location.X = -10
Lights(0).Location.Y = 180
Lights(0).Location.Z = 250
Lights(0).Intensity = 10

Lights(1).Location.X = 300
Lights(1).Location.Y = 40
Lights(1).Location.Z = 200
Lights(1).Intensity = 5

CameraZ! = 250

AmbientLight! = .2
Shininess! = 10

SCREEN 13

SetPalette

DEF SEG = &HA000

maxsize! = 5

DO
  DrawSphere RND * 320, RND * 200, (RND * .9 + .1) * maxsize!, RND * 10
  maxsize! = maxsize! * 1.005
LOOP WHILE INKEY$ = "" AND maxsize! < 75

SUB DrawSphere (centreX%, centreY%, r%, c%)
DIM centrepoint AS Vector
DIM surfacepoint AS Vector
DIM normal AS Vector
DIM reflection AS Vector
DIM tolight AS Vector
DIM tocamera AS Vector

c% = c% * 25
IF c% + 25 > 255 THEN c% = 0

centrepoint.X = centreX%
centrepoint.Y = centreY%

rsq% = r% * r%

ir! = 1! / r%

y1% = centreY% - r%
y2% = centreY% + r%

IF y1% < 0 THEN y1% = 0
IF y2% > 199 THEN y2% = 199

FOR py% = y1% TO y2%
  dy% = py% - centreY%

  surfacepoint.Y = py%

  ysq% = dy% * dy%

  dr% = SQR(r% * r% - ysq%)
  IF dr% * dr% + ysq% >= rsq% THEN dr% = dr% - 1

  IF dr% >= 0 THEN
    x1% = centreX% - dr%
    x2% = centreX% + dr%

    IF x1% < 0 THEN x1% = 0
    IF x2% > 319 THEN x2% = 319

    o& = py% * 320& + x1%

    FOR px% = x1% TO x2%
      dx% = px% - centreX%
      surfacepoint.X = px%
      surfacepoint.Z = SQR(rsq% - dx% * dx% - ysq%)

      tz% = surfacepoint.Z * 20

      normal = surfacepoint
      VecSub normal, centrepoint
      VecMul normal, ir!

      light! = AmbientLight!

      FOR i% = 0 TO UBOUND(Lights)
        tolight = Lights(i%).Location
        VecSub tolight, surfacepoint
        VecMul tolight, 1! / SQR(VecDot(tolight, tolight))

        tocamera.X = 160 - px%
        tocamera.Y = py% - 100
        tocamera.Z = CameraZ!

        VecMul tocamera, 1! / SQR(VecDot(tocamera, tocamera))

        reflection = normal
        VecMul reflection, 2 * VecDot(normal, tolight)
        VecSub reflection, tolight

        diffuse! = VecDot(tolight, normal)
        specular! = VecDot(reflection, tocamera) ^ Shininess!

        IF diffuse! < 0! THEN diffuse! = 0!
        IF specular! < 0! THEN specular! = 0!

        light! = light! + Lights(i%).Intensity * (diffuse! + specular!)
      NEXT i%

      i% = INT(light!)
      IF i% < 0 THEN i% = 0
      IF i% > 24 THEN i% = 24

      POKE o&, c% + i%
      o& = o& + 1
    NEXT px%
  END IF
NEXT py%
END SUB

SUB SetPalette
OUT &H3C8, 0

FOR hue! = 0 TO 359 STEP 36
  i1! = hue! * 64 / 60
  WHILE i1! > 64: i1! = i1! - 64: WEND

  i2! = 64 - i1!

  i1! = FIX(i1!): IF i1! > 63 THEN i1! = 63
  i2! = FIX(i2!): IF i2! > 63 THEN i2! = 63

  SELECT CASE hue!
    CASE IS < 60: r% = 63: g% = i1!: b% = 0
    CASE IS < 120: r% = i2!: g% = 63: b% = 0
    CASE IS < 180: r% = 0: g% = 63: b% = i1!
    CASE IS < 240: r% = 0: g% = i2!: b% = 63
    CASE IS < 300: r% = i1!: g% = 0: b% = 63
    CASE ELSE: r% = 63: g% = 0: b% = i2!
  END SELECT

  FOR i% = 0 TO 24
    OUT &H3C9, r% * i% \ 24
    OUT &H3C9, g% * i% \ 24
    OUT &H3C9, b% * i% \ 24
  NEXT i%
NEXT hue!
END SUB

SUB VecAdd (a AS Vector, b AS Vector)
a.X = a.X + b.X
a.Y = a.Y + b.Y
a.Z = a.Z + b.Z
END SUB

FUNCTION VecDot (a AS Vector, b AS Vector)
VecDot = a.X * b.X + a.Y * b.Y + a.Z * b.Z
END FUNCTION

SUB VecMul (a AS Vector, m AS SINGLE)
a.X = a.X * m
a.Y = a.Y * m
a.Z = a.Z * m
END SUB

SUB VecSub (a AS Vector, b AS Vector)
a.X = a.X - b.X
a.Y = a.Y - b.Y
a.Z = a.Z - b.Z
END SUB
