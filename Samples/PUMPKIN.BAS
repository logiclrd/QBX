' PUMPKIN.BAS -- QuickBASIC 4.5, VGA 320x200x256 (SCREEN 13)
' A low-resolution software rasterizer with a cut hollow shell,
' a twelve-lobed pumpkin, a curved stem, and lit 3-D Perlin fog.
' Run in QB 4.5 or QB 7.1; compile with BC /AH when necessary.
' No external libraries are required.
REM $DYNAMIC

CONST PI = 3.141592654#
CONST TWOPI = 6.283185307#
CONST NA = 72
CONST NL = 20
CONST SW = 320
CONST SH = 200
CONST MAXV = 3200
CONST MAXF = 7000
CONST CAMZ = 4.8
CONST FOCAL = 200!
CONST YSQUASH = .85
' The finished pumpkin's mean equatorial radius is approximately 1.30.
' Stem radius is 10% of that; its length is four radii.
CONST STEMR = .13
CONST STEMLEN = .52
CONST STEPBEND = .2617994
CONST SAMPLES = 24
CONST FILLX = 5!
CONST FILLY = 1.5
CONST FILLZ = 1.5
CONST RESOLUTION% = 1 ' or 2

TYPE VERTEX3
    x AS SINGLE
    y AS SINGLE
    z AS SINGLE
END TYPE

TYPE FACE3
    a AS INTEGER
    b AS INTEGER
    c AS INTEGER
    material AS INTEGER
END TYPE

DECLARE FUNCTION NewV% (x!, y!, z!)
DECLARE SUB NewF (a%, b%, c%, material%)
DECLARE SUB CutWall (a%, b%, shellN%)
DECLARE FUNCTION VIndex% (i%, j%, layer%)
DECLARE SUB MakeHoles ()
DECLARE SUB MakeShell ()
DECLARE SUB MakeStem ()
DECLARE SUB RotateMeshForPresentation ()
DECLARE SUB RotateForPresentation ()
DECLARE FUNCTION InPoly% (x!, y!, k%)
DECLARE FUNCTION IsHole% (x!, y!, z!)
DECLARE FUNCTION Angle! (x!, z!)
DECLARE FUNCTION Radial! (a!)
DECLARE FUNCTION OuterRad! (x!, y!, z!)
DECLARE SUB InitNoise ()
DECLARE FUNCTION Fade! (t!)
DECLARE FUNCTION Grad! (h%, x!, y!, z!)
DECLARE FUNCTION Noise3! (x!, y!, z!)
DECLARE FUNCTION FogD! (x!, y!, z!)
DECLARE FUNCTION Lit% (BYVAL x!, BYVAL y!, BYVAL z!)
DECLARE FUNCTION HueValue! (a!, b!, h!)
DECLARE SUB MakePalette ()
DECLARE SUB Triangle (fi%)
DECLARE SUB RenderFog ()

DIM SHARED V(1 TO MAXV) AS VERTEX3
DIM SHARED F(1 TO MAXF) AS FACE3
DIM SHARED NV%, NF%
DIM SHARED Cut%(0 TO NA - 1, 0 TO NL - 1)
DIM SHARED PN%(1 TO 4)
DIM SHARED px!(1 TO 16, 1 TO 4)
DIM SHARED py!(1 TO 16, 1 TO 4)
DIM SHARED P%(0 TO 511)
DIM SHARED ZB%(0 TO SW - 1, 0 TO SH - 1)
DIM SHARED BASEC%(0 TO SW - 1, 0 TO SH - 1)
DIM SHARED StemBase!
DIM SHARED ctilt!, stilt!
DIM SHARED cturn!, sturn!

atilt! = 10 / 57.295779#
aturn! = 15 / 57.295779#

ctilt! = COS(atilt!)
stilt! = SIN(atilt!)

cturn! = COS(aturn!)
sturn! = SIN(aturn!)

RANDOMIZE TIMER
CALL MakeHoles
CALL MakeShell
CALL MakeStem
CALL RotateMeshForPresentation
CALL InitNoise

SCREEN 13
CALL MakePalette
FOR ii% = 1 TO NF%
    CALL Triangle(ii%)
NEXT ii%
CALL RenderFog
DEF SEG
DO
    k$ = INKEY$
LOOP UNTIL k$ <> ""
SCREEN 0
WIDTH 80
END

FUNCTION Angle! (x!, z!)
    IF z! = 0! THEN
        IF x! >= 0! THEN
            t! = PI / 2!
        ELSE
            t! = PI * 1.5
        END IF
    ELSE
        t! = ATN(x! / z!)
        IF z! < 0! THEN t! = t! + PI
        IF t! < 0! THEN t! = t! + TWOPI
    END IF
    Angle! = t!
END FUNCTION

SUB CutWall (a%, b%, shellN%)
    ' A shared edge between a missing cell and a surviving cell.
    ' The two triangles link outer and inner surfaces into a rim.
    CALL NewF(a%, b%, b% + shellN%, 1)
    CALL NewF(a%, b% + shellN%, a% + shellN%, 1)
END SUB

FUNCTION Fade! (t!)
    Fade! = t! * t! * t! * (t! * (t! * 6! - 15!) + 10!)
END FUNCTION

FUNCTION FogD! (x!, y!, z!)
    ' Two octaves of 3-D gradient Perlin noise.
    n! = .7 * Noise3!(x! * 2.6, y! * 2.6, z! * 2.6)
    n! = n! + .3 * Noise3!(x! * 5.2 + 17!, y! * 5.2 + 9!, z! * 5.2 + 13!)
    d! = .65 + .45 * n!
    IF d! < 0! THEN d! = 0!
    IF d! > 1! THEN d! = 1!
    ' Sharpen the density distribution: thin smoke becomes much thinner,
    ' while the densest patches retain most of their opacity.
    d2! = d! * d!
    FogD! = d2! * d2! * d!
END FUNCTION

FUNCTION Grad! (h%, x!, y!, z!)
    h% = h% AND 15
    IF h% < 8 THEN uu! = x! ELSE uu! = y!
    IF h% < 4 THEN
        vv! = y!
    ELSEIF h% = 12 OR h% = 14 THEN
        vv! = x!
    ELSE
        vv! = z!
    END IF
    IF (h% AND 1) <> 0 THEN uu! = -uu!
    IF (h% AND 2) <> 0 THEN vv! = -vv!
    Grad! = uu! + vv!
END FUNCTION

FUNCTION HueValue! (a!, b!, h!)
    IF h! < 0! THEN h! = h! + 1!
    IF h! > 1! THEN h! = h! - 1!
    IF h! < 1! / 6! THEN
        HueValue! = a! + (b! - a!) * 6! * h!
    ELSEIF h! < .5 THEN
        HueValue! = b!
    ELSEIF h! < 2! / 3! THEN
        HueValue! = a! + (b! - a!) * (2! / 3! - h!) * 6!
    ELSE
        HueValue! = a!
    END IF
END FUNCTION

SUB InitNoise
    FOR i% = 0 TO 255
        P%(i%) = i%
    NEXT i%
    FOR i% = 255 TO 1 STEP -1
        j% = INT(RND * (i% + 1))
        tmp% = P%(i%): P%(i%) = P%(j%): P%(j%) = tmp%
    NEXT i%
    FOR i% = 0 TO 255
        P%(256 + i%) = P%(i%)
    NEXT i%
END SUB

FUNCTION InPoly% (x!, y!, k%)
    inside% = 0
    FOR p1% = 1 TO PN%(k%)
        p2% = p1% + 1
        IF p2% > PN%(k%) THEN p2% = 1
        y1! = py!(p1%, k%): y2! = py!(p2%, k%)
        IF ((y1! > y!) <> (y2! > y!)) THEN
            xCross! = px!(p1%, k%) + (y! - y1!) * (px!(p2%, k%) - px!(p1%, k%)) / (y2! - y1!)
            IF x! < xCross! THEN inside% = 1 - inside%
        END IF
    NEXT p1%
    InPoly% = inside%
END FUNCTION

FUNCTION IsHole% (x!, y!, z!)
    IsHole% = 0
    IF z! <= .001 THEN EXIT FUNCTION
    u! = x! / z!: V! = y! / z!
    FOR k% = 1 TO 4
        IF InPoly%(u!, V!, k%) <> 0 THEN
            IsHole% = -1
            EXIT FUNCTION
        END IF
    NEXT k%
END FUNCTION

FUNCTION Lit% (BYVAL x!, BYVAL y!, BYVAL z!)
    ' Light is inside the shell. The front polygons are angular apertures
    ' extending radially from the origin, so they cast conical light beams.
    RotatePointForPresentation x!, y!, z!
    yp! = y! / YSQUASH
    d! = SQR(x! * x! + z! * z! + yp! * yp!)
    IF d! < .88 * OuterRad!(x!, y!, z!) THEN
        Lit% = -1
    ELSEIF IsHole%(x!, y!, z!) THEN
        Lit% = -1
    ELSE
        Lit% = 0
    END IF
END FUNCTION

SUB MakeHoles
    ' Four polygons in tangent-plane coordinates u=x/z, v=y/z.
    ' Positive z is the face of the pumpkin turned toward the camera.
    PN%(1) = 3
    px!(1, 1) = -.61: py!(1, 1) = .27
    px!(2, 1) = -.18: py!(2, 1) = .26
    px!(3, 1) = -.42: py!(3, 1) = .71

    PN%(2) = 3
    px!(1, 2) = .18: py!(1, 2) = .26
    px!(2, 2) = .61: py!(2, 2) = .27
    px!(3, 2) = .42: py!(3, 2) = .71

    PN%(3) = 3
    px!(1, 3) = -.13: py!(1, 3) = -.03
    px!(2, 3) = .13: py!(2, 3) = -.03
    px!(3, 3) = 0!: py!(3, 3) = .23

    PN%(4) = 14
    px!(1, 4) = -.76: py!(1, 4) = -.16
    px!(2, 4) = -.54: py!(2, 4) = -.26
    px!(3, 4) = -.35: py!(3, 4) = -.27
    px!(4, 4) = -.22: py!(4, 4) = -.37
    px!(5, 4) = 0!: py!(5, 4) = -.3
    px!(6, 4) = .22: py!(6, 4) = -.37
    px!(7, 4) = .35: py!(7, 4) = -.27
    px!(8, 4) = .54: py!(8, 4) = -.26
    px!(9, 4) = .76: py!(9, 4) = -.16
    px!(10, 4) = .62: py!(10, 4) = -.54
    px!(11, 4) = .34: py!(11, 4) = -.67
    px!(12, 4) = 0!: py!(12, 4) = -.73
    px!(13, 4) = -.34: py!(13, 4) = -.67
    px!(14, 4) = -.62: py!(14, 4) = -.54
END SUB

SUB MakePalette
    ' Index = 16 * fog row + material/light column.
    ' Columns 0-12: pumpkin H=38, L=0..128.
    ' Columns 13-15: stem H=38..124, L=128..80.
    ' Row 15: L=192, S=0; fully fogged colors become gray.
    ' Hue values are degrees on a 0..360 wheel.
    OUT &H3C8, 0
    FOR fog% = 0 TO 15
        blend! = fog% / 15!
        sat! = 1! - blend!
        FOR shade% = 0 TO 15
            IF shade% <= 12 THEN
                hue! = 38!
                l0! = shade% * 128! / 12!
            ELSE
                F! = (shade% - 12!) / 3!
                hue! = 38! + F! * (124! - 38!)
                l0! = 128! + SQR(F!) * (60! - 128!)
            END IF
            h! = hue! / 360!
            lum! = (l0! + (192! - l0!) * blend!) / 255!
            IF lum! <= .5 THEN
                b! = lum! * (1! + sat!)
            ELSE
                b! = lum! + sat! - lum! * sat!
            END IF
            a! = 2! * lum! - b!
            rr! = HueValue!(a!, b!, h! + 1! / 3!)
            gg! = HueValue!(a!, b!, h!)
            bb! = HueValue!(a!, b!, h! - 1! / 3!)
            OUT &H3C9, CINT(63! * rr!)
            OUT &H3C9, CINT(63! * gg!)
            OUT &H3C9, CINT(63! * bb!)
        NEXT shade%
    NEXT fog%
END SUB

SUB MakeShell
    ' Two matching closed surfaces, each with shared pole vertices.
    ' Angular cut cells are omitted and their boundary gets rim walls.
    band% = NA \ 12
    FOR layer% = 0 TO 1
        r! = Radial!(-PI / 2!)
        IF layer% = 1 THEN r! = .88 * r!
        dummy% = NewV%(0!, -YSQUASH * r!, 0!)
        FOR j% = 1 TO NL - 1
            a! = -PI / 2! + PI * j% / NL
            ca! = COS(a!): sa! = SIN(a!)
            FOR i% = 0 TO NA - 1
                th! = TWOPI * i% / NA
                phase! = (i% MOD band%) / band%
                rib! = 1! + .13 * SIN(PI * phase!)
                r! = Radial!(a!) * rib!
                IF layer% = 1 THEN r! = r! * .88
                xx! = r! * ca! * SIN(th!)
                yy! = YSQUASH * r! * sa!
                zz! = r! * ca! * COS(th!)
                dummy% = NewV%(xx!, yy!, zz!)
            NEXT i%
        NEXT j%
        r! = Radial!(PI / 2!)
        IF layer% = 1 THEN r! = .88 * r!
        dummy% = NewV%(0!, YSQUASH * r!, 0!)
    NEXT layer%

    shellN% = (NL - 1) * NA + 2
    StemBase! = YSQUASH * Radial!(PI / 2!)

    FOR j% = 0 TO NL - 1
        a! = -PI / 2! + PI * (j% + .5) / NL
        FOR i% = 0 TO NA - 1
            th! = TWOPI * (i% + .5) / NA
            xx! = COS(a!) * SIN(th!)
            yy! = YSQUASH * SIN(a!)
            zz! = COS(a!) * COS(th!)
            IF IsHole%(xx!, yy!, zz!) THEN Cut%(i%, j%) = 1
        NEXT i%
    NEXT j%

    FOR j% = 0 TO NL - 1
        FOR i% = 0 TO NA - 1
            in% = (i% + 1) MOD NA
            a% = VIndex%(i%, j%, 0)
            b% = VIndex%(in%, j%, 0)
            c% = VIndex%(in%, j% + 1, 0)
            d% = VIndex%(i%, j% + 1, 0)
            IF Cut%(i%, j%) = 0 THEN
                IF j% > 0 THEN CALL NewF(a%, b%, c%, 1)
                IF j% < NL - 1 THEN CALL NewF(a%, c%, d%, 1)
                IF j% > 0 THEN
                    CALL NewF(a% + shellN%, c% + shellN%, b% + shellN%, 1)
                END IF
                IF j% < NL - 1 THEN
                    CALL NewF(a% + shellN%, d% + shellN%, c% + shellN%, 1)
                END IF
            END IF
        NEXT i%
    NEXT j%

    FOR j% = 0 TO NL - 1
        FOR i% = 0 TO NA - 1
            IF Cut%(i%, j%) THEN
                in% = (i% + 1) MOD NA
                ip% = (i% + NA - 1) MOD NA
                a% = VIndex%(i%, j%, 0)
                b% = VIndex%(in%, j%, 0)
                c% = VIndex%(in%, j% + 1, 0)
                d% = VIndex%(i%, j% + 1, 0)
                IF j% > 0 THEN
                    IF Cut%(i%, j% - 1) = 0 THEN
                        CALL CutWall(a%, b%, shellN%)
                    END IF
                END IF
                IF Cut%(in%, j%) = 0 THEN CALL CutWall(b%, c%, shellN%)
                IF j% < NL - 1 THEN
                    IF Cut%(i%, j% + 1) = 0 THEN
                        CALL CutWall(c%, d%, shellN%)
                    END IF
                END IF
                IF Cut%(ip%, j%) = 0 THEN CALL CutWall(d%, a%, shellN%)
            END IF
        NEXT i%
    NEXT j%
END SUB

SUB MakeStem
    ' Six sections; the tangent rotates by a total of 15 degrees.
    ' Curvature is integrated so the centerline stays continuous.
    start% = NV% + 1
    FOR j% = 0 TO 6
        t! = j% / 6!
        phi! = STEPBEND * t!
        cx! = STEMLEN * (1! - COS(phi!)) / STEPBEND
        cy! = StemBase! + STEMLEN * SIN(phi!) / STEPBEND
        FOR i% = 0 TO 11
            az! = TWOPI * i% / 12!
            xx! = cx! + STEMR * COS(phi!) * COS(az!)
            yy! = cy! - STEMR * SIN(phi!) * COS(az!)
            zz! = STEMR * SIN(az!)
            dummy% = NewV%(xx!, yy!, zz!)
        NEXT i%
    NEXT j%
    FOR j% = 0 TO 5
        FOR i% = 0 TO 11
            in% = (i% + 1) MOD 12
            a% = start% + j% * 12 + i%
            b% = start% + j% * 12 + in%
            c% = b% + 12
            d% = a% + 12
            CALL NewF(a%, d%, c%, 2)
            CALL NewF(a%, c%, b%, 2)
        NEXT i%
    NEXT j%
    top% = NewV%(STEMLEN * (1! - COS(STEPBEND)) / STEPBEND, StemBase! + STEMLEN * SIN(STEPBEND) / STEPBEND, 0!)
    FOR i% = 0 TO 11
        in% = (i% + 1) MOD 12
        CALL NewF(top%, start% + 6 * 12 + i%, start% + 6 * 12 + in%, 2)
    NEXT i%
END SUB

SUB NewF (a%, b%, c%, material%)
    IF NF% >= MAXF THEN
        SCREEN 0
        PRINT "Increase MAXF"
        END
    END IF
    NF% = NF% + 1
    F(NF%).a = a%
    F(NF%).b = b%
    F(NF%).c = c%
    F(NF%).material = material%
END SUB

FUNCTION NewV% (x!, y!, z!)
    IF NV% >= MAXV THEN
        SCREEN 0
        PRINT "Increase MAXV"
        END
    END IF
    NV% = NV% + 1
    V(NV%).x = x!
    V(NV%).y = y!
    V(NV%).z = z!
    NewV% = NV%
END FUNCTION

FUNCTION Noise3! (x!, y!, z!)
    ix% = INT(x!): iy% = INT(y!): iz% = INT(z!)
    fx! = x! - ix%: fy! = y! - iy%: fz! = z! - iz%
    xx% = ix% AND 255: yy% = iy% AND 255
    zz% = iz% AND 255
    u! = Fade!(fx!): V! = Fade!(fy!): w! = Fade!(fz!)
    a% = P%(xx%) + yy%: b% = P%(xx% + 1) + yy%
    aa% = P%(a%) + zz%: ab% = P%(a% + 1) + zz%
    ba% = P%(b%) + zz%: bb% = P%(b% + 1) + zz%

    g000! = Grad!(P%(aa%), fx!, fy!, fz!)
    g100! = Grad!(P%(ba%), fx! - 1!, fy!, fz!)
    g010! = Grad!(P%(ab%), fx!, fy! - 1!, fz!)
    g110! = Grad!(P%(bb%), fx! - 1!, fy! - 1!, fz!)
    g001! = Grad!(P%(aa% + 1), fx!, fy!, fz! - 1!)
    g101! = Grad!(P%(ba% + 1), fx! - 1!, fy!, fz! - 1!)
    g011! = Grad!(P%(ab% + 1), fx!, fy! - 1!, fz! - 1!)
    g111! = Grad!(P%(bb% + 1), fx! - 1!, fy! - 1!, fz! - 1!)

    xa! = g000! + u! * (g100! - g000!)
    xb! = g010! + u! * (g110! - g010!)
    xc! = g001! + u! * (g101! - g001!)
    xd! = g011! + u! * (g111! - g011!)
    ya! = xa! + V! * (xb! - xa!)
    yb! = xc! + V! * (xd! - xc!)
    Noise3! = ya! + w! * (yb! - ya!)
END FUNCTION

FUNCTION OuterRad! (x!, y!, z!)
    ' Radius of the deformed shell along an origin-to-point ray.
    yp! = y! / YSQUASH
    horiz! = SQR(x! * x! + z! * z!)
    IF horiz! < .00001 THEN
        IF yp! >= 0! THEN a! = PI / 2! ELSE a! = -PI / 2!
    ELSE
        a! = ATN(yp! / horiz!)
    END IF
    th! = Angle!(x!, z!)
    bandangle! = TWOPI / 12!
    phase! = (th! - INT(th! / bandangle!) * bandangle!) / bandangle!
    rib! = 1! + .13 * SIN(PI * phase!)
    OuterRad! = Radial!(a!) * rib!
END FUNCTION

FUNCTION Radial! (a!)
    ' The prompt's x is interpreted as elevation a IN RADIANS.
    ' The 1 and -1 cancel, but the expression is kept verbatim.
    Radial! = COS(1! - COS(1.5 * a!) - 1!) * .4 + 1!
END FUNCTION

SUB RenderFog
    ' Integrate density from the camera to the nearest surface.
    ' Stronger beam weighting makes carved openings visibly project
    ' light through the fog, while a lower base term preserves shadow.
    DEF SEG = &HA000
    FOR py% = 0 TO SH - 1 STEP RESOLUTION%
        FOR px% = 0 TO SW - 1 STEP RESOLUTION%
            IF ZB%(px%, py%) > 0 THEN
                tmax! = 20000! / ZB%(px%, py%)
            ELSE
                tmax! = 7!
            END IF
            dt! = tmax! / SAMPLES
            dx! = (px% + .5 - SW / 2!) / FOCAL
            dy! = -(py% + .5 - SH / 2!) / FOCAL
            jitter! = (RND - .5) * .45
            optical! = 0!
            brightOpt! = 0!
            FOR s% = 0 TO SAMPLES - 1
                t! = (s% + .5 + jitter!) * dt!
                sx! = t! * dx!
                sy! = t! * dy!
                sz! = CAMZ - t!
                d! = FogD!(sx!, sy!, sz!)
                stepOpt! = d! * dt! / 4!
                optical! = optical! + stepOpt!

                litFrac! = 0!
                lt! = (s% + .15 + jitter!) * dt!
                IF Lit%(lt! * dx!, lt! * dy!, CAMZ - lt!) THEN litFrac! = litFrac! + .3333
                lt! = (s% + .5 + jitter!) * dt!
                IF Lit%(lt! * dx!, lt! * dy!, CAMZ - lt!) THEN litFrac! = litFrac! + .3334
                lt! = (s% + .85 + jitter!) * dt!
                IF Lit%(lt! * dx!, lt! * dy!, CAMZ - lt!) THEN litFrac! = litFrac! + .3333
                brightOpt! = brightOpt! + stepOpt! * litFrac!
            NEXT s%
            IF optical! > 1! THEN optical! = 1!
            IF brightOpt! > 1! THEN brightOpt! = 1!
            fog! = .16 * optical! + 2.65 * brightOpt!
            IF fog! > 1! THEN fog! = 1!
            row% = INT(15! * fog! + .5)
            col% = BASEC%(px%, py%)
            ink% = row% * 16 + col%
            offset& = CLNG(py%) * 320& + CLNG(px%)
            POKE offset&, ink%

            IF RESOLUTION% = 2 THEN
                POKE offset& + 1, ink%
                POKE offset& + 320, ink%
                POKE offset& + 321, ink%
            END IF
        NEXT px%
    NEXT py%
    DEF SEG
END SUB

SUB RotateMeshForPresentation
FOR i% = 1 TO NV%
  x! = V(i%).x
  y! = V(i%).y
  z! = V(i%).z

  RotatePointForPresentation x!, y!, z!

  V(i%).x = x!
  V(i%).y = y!
  V(i%).z = z!
NEXT i%
END SUB

SUB RotatePointForPresentation (x!, y!, z!)
' First, apply X axis tilt
ny! = y! * ctilt! - z! * stilt!
nz! = y! * stilt! + z! * ctilt!
y! = ny!: z! = nz!

' Then, top-down rotation
nx! = x! * cturn! - z! * sturn!
nz! = x! * sturn! + z! * cturn!
x! = nx!: z! = nz!
END SUB

SUB Triangle (fi%)
    a% = F(fi%).a: b% = F(fi%).b: c% = F(fi%).c
    z1! = CAMZ - V(a%).z
    z2! = CAMZ - V(b%).z
    z3! = CAMZ - V(c%).z
    IF z1! <= .01 OR z2! <= .01 OR z3! <= .01 THEN EXIT SUB
    x1! = SW / 2! + FOCAL * V(a%).x / z1!
    y1! = SH / 2! - FOCAL * V(a%).y / z1!
    x2! = SW / 2! + FOCAL * V(b%).x / z2!
    y2! = SH / 2! - FOCAL * V(b%).y / z2!
    x3! = SW / 2! + FOCAL * V(c%).x / z3!
    y3! = SH / 2! - FOCAL * V(c%).y / z3!
    xmin% = INT(x1!): xmax% = INT(x1!)
    ymin% = INT(y1!): ymax% = INT(y1!)
    IF INT(x2!) < xmin% THEN xmin% = INT(x2!)
    IF INT(x3!) < xmin% THEN xmin% = INT(x3!)
    IF INT(x2!) > xmax% THEN xmax% = INT(x2!)
    IF INT(x3!) > xmax% THEN xmax% = INT(x3!)
    IF INT(y2!) < ymin% THEN ymin% = INT(y2!)
    IF INT(y3!) < ymin% THEN ymin% = INT(y3!)
    IF INT(y2!) > ymax% THEN ymax% = INT(y2!)
    IF INT(y3!) > ymax% THEN ymax% = INT(y3!)
    IF xmin% < 0 THEN xmin% = 0
    IF ymin% < 0 THEN ymin% = 0
    IF xmax% > SW - 1 THEN xmax% = SW - 1
    IF ymax% > SH - 1 THEN ymax% = SH - 1
    IF xmin% > xmax% OR ymin% > ymax% THEN EXIT SUB

    den! = (y2! - y3!) * (x1! - x3!) + (x3! - x2!) * (y1! - y3!)
    IF ABS(den!) < .00001 THEN EXIT SUB
    q1! = 20000! / z1!: q2! = 20000! / z2!
    q3! = 20000! / z3!

    ex! = V(b%).x - V(a%).x: ey! = V(b%).y - V(a%).y
    ez! = V(b%).z - V(a%).z
    fx! = V(c%).x - V(a%).x: fy! = V(c%).y - V(a%).y
    fz! = V(c%).z - V(a%).z
    nx! = ey! * fz! - ez! * fy!
    ny! = ez! * fx! - ex! * fz!
    nz! = ex! * fy! - ey! * fx!
    nlen! = SQR(nx! * nx! + ny! * ny! + nz! * nz!)
    IF nlen! < .000001 THEN EXIT SUB
    nx! = nx! / nlen!: ny! = ny! / nlen!: nz! = nz! / nlen!

    cx! = (V(a%).x + V(b%).x + V(c%).x) / 3!
    cy! = (V(a%).y + V(b%).y + V(c%).y) / 3!
    cz! = (V(a%).z + V(b%).z + V(c%).z) / 3!

    vx! = -cx!: vy! = -cy!: vz! = CAMZ - cz!
    vlen! = SQR(vx! * vx! + vy! * vy! + vz! * vz!)
    IF vlen! < .000001 THEN vlen! = 1!
    vx! = vx! / vlen!: vy! = vy! / vlen!: vz! = vz! / vlen!
    IF nx! * vx! + ny! * vy! + nz! * vz! < 0! THEN
        nx! = -nx!: ny! = -ny!: nz! = -nz!
    END IF

    bright! = .2

    dist0! = SQR(cx! * cx! + cy! * cy! + cz! * cz!)
    IF dist0! > .0001 THEN
        lx0! = -cx! / dist0!
        ly0! = -cy! / dist0!
        lz0! = -cz! / dist0!
        diff0! = nx! * lx0! + ny! * ly0! + nz! * lz0!
        IF diff0! > 0! THEN bright! = bright! + .6 * diff0!
    END IF

    lx1! = FILLX - cx!: ly1! = FILLY - cy!: lz1! = FILLZ - cz!
    llen1! = SQR(lx1! * lx1! + ly1! * ly1! + lz1! * lz1!)
    IF llen1! < .0001 THEN llen1! = 1!
    lx1! = lx1! / llen1!: ly1! = ly1! / llen1!: lz1! = lz1! / llen1!
    diff1! = nx! * lx1! + ny! * ly1! + nz! * lz1!
    IF diff1! > 0! THEN bright! = bright! + .2 * diff1!

    IF F(fi%).material = 1 THEN
        specPow! = 5!
        specScale! = .18
    ELSE
        specPow! = .5
        specScale! = .1
    END IF
    IF diff1! > 0! THEN
        rx! = 2! * diff1! * nx! - lx1!
        ry! = 2! * diff1! * ny! - ly1!
        rz! = 2! * diff1! * nz! - lz1!
        specDot! = rx! * vx! + ry! * vy! + rz! * vz!
        IF specDot! > 0! THEN
            IF specPow! = .5 THEN
                spec! = SQR(specDot!)
            ELSE
                spec! = specDot! ^ specPow!
            END IF
            bright! = bright! + specScale! * spec!
        END IF
    END IF
    IF bright! > 1! THEN bright! = 1!
    IF bright! < 0! THEN bright! = 0!
    shade% = INT(12! * bright! + .5)
    IF shade% > 12 THEN shade% = 12

    FOR py% = ymin% TO ymax%
        yy! = py% + .5
        FOR px% = xmin% TO xmax%
            xx! = px% + .5
            w1! = ((y2! - y3!) * (xx! - x3!) + (x3! - x2!) * (yy! - y3!)) / den!
            IF w1! >= -.00001 THEN
                w2! = ((y3! - y1!) * (xx! - x3!) + (x1! - x3!) * (yy! - y3!)) / den!
                IF w2! >= -.00001 THEN
                    w3! = 1! - w1! - w2!
                    IF w3! >= -.00001 THEN
                        inv% = INT(q1! * w1! + q2! * w2! + q3! * w3! + .5)
                        IF inv% > ZB%(px%, py%) THEN
                            ZB%(px%, py%) = inv%
                            IF F(fi%).material = 1 THEN
                                BASEC%(px%, py%) = shade%
                            ELSE
                                yyStem! = (w1! * V(a%).y * q1! + w2! * V(b%).y * q2! + w3! * V(c%).y * q3!) / (w1! * q1! + w2! * q2! + w3! * q3!)
                                ht! = (yyStem! - StemBase!) / STEMLEN
                                IF ht! >= .2 THEN
                                    stemCol% = 15
                                ELSEIF ht! >= .1 THEN
                                    stemCol% = 14
                                ELSE
                                    stemCol% = 13
                                END IF
                                stemBoost% = INT(2! * (bright! - .2) + .5)
                                IF stemBoost% < 0 THEN stemBoost% = 0
                                stemCol% = stemCol% + stemBoost%
                                IF stemCol% > 15 THEN stemCol% = 15
                                BASEC%(px%, py%) = stemCol%
                            END IF
                        END IF
                    END IF
                END IF
            END IF
        NEXT px%
    NEXT py%
END SUB

SUB UnrotatePointForPresentation (x!, y!, z!)
' Opposite order of RotatePointForPresentation:
' First, undo top-down rotation
nx! = z! * sturn! + x! * cturn!
nz! = z! * cturn! - x! * sturn!
x! = nx!: z! = nz!

' Then, undo X axis tilt
ny! = z! * stilt! + y! * ctilt!
nz! = z! * ctilt! - y! * stilt!
y! = ny!: z! = nz!
END SUB

FUNCTION VIndex% (i%, j%, layer%)
    ' Each layer has exactly ONE vertex at each pole.
    group% = (NL - 1) * NA + 2
    BASEC% = layer% * group%
    IF j% = 0 THEN
        VIndex% = BASEC% + 1
    ELSEIF j% = NL THEN
        VIndex% = BASEC% + group%
    ELSE
        VIndex% = BASEC% + 2 + (j% - 1) * NA + (i% MOD NA)
    END IF
END FUNCTION
