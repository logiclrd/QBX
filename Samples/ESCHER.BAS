' ================================================================
' Regular hyperbolic tiling & Escher tesselation
'
' Tiling:
'   Default: {3,7}
'     3 sides per polygon
'     7 polygons meeting at each vertex
'
'   The tiling itself lives entirely in the hyperboloid model.
'   It is projected into either the Poincare or Klein disk only
'   when it is drawn.
'
' Escher tesselation:
'   Side length is chosen to produce the same scale as David
'   Malnprize's original Sinclair SuperBASIC sample.
'
' The meat of this code was produced by ChatGPT. Configuration of
' parameters and some UI mechanics were overlaid by hand.
' ================================================================

TYPE HPoint
    x AS DOUBLE
    y AS DOUBLE
    t AS DOUBLE
END TYPE

CONST PI = 3.14159265358979#
CONST POLYSIDES = 3
CONST VERTEXQ = 7

CONST KLEIN = 0
CONST POINCARE = 1

CONST TILING = 0
CONST SUBDIVISION = 1

' Enough for the default {3,7} example and MAXDIST = 4.7.
CONST MAXTILES = 1200
CONST MAXSTACK = 256

' Number of straight screen segments used to approximate a
' projected Poincare geodesic.
CONST EDGESEGS = 12

DECLARE FUNCTION ACosh# (x#)
DECLARE FUNCTION HSinh# (x#)
DECLARE FUNCTION MinkDot# (a AS HPoint, b AS HPoint)
DECLARE FUNCTION AlreadySeen% (p AS HPoint, count%)

DECLARE SUB CopyPoint (a AS HPoint, b AS HPoint)
DECLARE SUB NormalizePoint (p AS HPoint)
DECLARE SUB EdgeNormal (a AS HPoint, b AS HPoint, n AS HPoint)
DECLARE SUB ReflectPoint (p AS HPoint, n AS HPoint, r AS HPoint)

DECLARE SUB ProjectPoint (p AS HPoint, model%, sx!, sy!)
DECLARE SUB DrawEdge (a AS HPoint, b AS HPoint, model%, col%)
DECLARE SUB DrawPolygon (v() AS HPoint, model%, col%)
DECLARE SUB GenerateTiling (model%, tileCount%, truncated%)
DECLARE SUB RingPoint (tier%, index%, edgeLength#, p AS HPoint)
DECLARE SUB DrawEscherTesselation (maxTier%, model%, edgeLength#, tileCount%, truncated%)

DIM SHARED scrCX AS INTEGER
DIM SHARED scrCY AS INTEGER
DIM SHARED scrR AS INTEGER

' Explicit traversal stack.  We need retain only polygons which
' have not yet been processed.
DIM SHARED stackV(0 TO MAXSTACK - 1, 0 TO POLYSIDES - 1) AS HPoint
DIM SHARED stackC(0 TO MAXSTACK - 1) AS HPoint
DIM SHARED stackSkip(0 TO MAXSTACK - 1) AS INTEGER

' To recognize polygons reached by two different paths, storing
' only their centre in Poincare coordinates is sufficient.
DIM SHARED seenX(0 TO MAXTILES - 1) AS DOUBLE
DIM SHARED seenY(0 TO MAXTILES - 1) AS DOUBLE


' ------------------------------------------------
' Main program
' ------------------------------------------------

SCREEN 12

scrCX = 320
scrCY = 240
scrR = 220

model% = KLEIN
figure% = TILING

maxTier% = 5

' Derivation:
'
'               .-                      -.
'               |      pi       r        |
'   L = 2 asinh |  sin -- -------------  |
'               |      12 sqrt(1 - r^2)  |
'               `-                      -'
'
' where r is the initial _projected_ radius.

edgeLength# = .3201520056#

DO
    CLS

    IF model% = POINCARE THEN
        LOCATE 1, 1
        PRINT "Poincare disk -- ";
    ELSE
        LOCATE 1, 1
        PRINT "Klein disk -- ";
    END IF

    SELECT CASE figure%
        CASE TILING:
            PRINT "Tiling: {"; LTRIM$(STR$(POLYSIDES)); ","; LTRIM$(STR$(VERTEXQ)); "}"
            GenerateTiling model%, tileCount%, truncated%
        CASE SUBDIVISION:
            PRINT "Escher tesselation: "; LTRIM$(STR$(maxTier%)); " tiers with edge length "; LTRIM$(STR$(edgeLength#)); "}"
            DrawEscherTesselation maxTier%, model%, edgeLength#, tileCount%, truncated%
    END SELECT

    ' Boundary at hyperbolic infinity.
    CIRCLE (scrCX, scrCY), scrR, 8

    LOCATE 30, 1
    PRINT tileCount%; " polygons [ T=Tiling, S=Escher Tesselation | P=Poincare, K=Klein ]   Q=quit";

    IF truncated% THEN PRINT "   [TRUNCATED]";

    DO
        k$ = UCASE$(INKEY$)
    LOOP WHILE k$ = ""

    SELECT CASE k$
        CASE "T": figure% = TILING
        CASE "S": figure% = SUBDIVISION

        CASE "P": model% = POINCARE
        CASE "K": model% = KLEIN

        CASE "Q", CHR$(27): EXIT DO
    END SELECT
LOOP

SCREEN 0
END


FUNCTION ACosh# (x#)

    IF x# < 1# THEN x# = 1#

    ACosh# = LOG(x# + SQR(x# * x# - 1#))

END FUNCTION

FUNCTION AlreadySeen% (p AS HPoint, count%)

    ' Use Poincare coordinates for numerical duplicate detection.
    ' This has nothing to do with construction of the geometry.

    px# = p.x / (p.t + 1#)
    py# = p.y / (p.t + 1#)

    epsilon# = .00000001#

    FOR i% = 0 TO count% - 1

        dx# = px# - seenX(i%)
        dy# = py# - seenY(i%)

        IF dx# * dx# + dy# * dy# < epsilon# * epsilon# THEN
            AlreadySeen% = -1
            EXIT FUNCTION
        END IF

    NEXT i%

    AlreadySeen% = 0

END FUNCTION

SUB CopyPoint (a AS HPoint, b AS HPoint)

    b.x = a.x
    b.y = a.y
    b.t = a.t

END SUB

' ================================================================
' Rendering geodesics
' ================================================================
SUB DrawEdge (a AS HPoint, b AS HPoint, model%, col%)

    ' In the Klein model a hyperbolic geodesic projects EXACTLY
    ' to a straight Euclidean chord.

    IF model% = KLEIN THEN

        ProjectPoint a, model%, x1!, y1!
        ProjectPoint b, model%, x2!, y2!

        LINE (x1!, y1!)-(x2!, y2!), col%

        EXIT SUB
    END IF

    ' In the Poincare model the edge becomes a circular arc.
    '
    ' Rather than deriving that screen-space circle, stay completely
    ' in hyperbolic space.  Interpolate along the actual hyperbolic
    ' geodesic and project each resulting HPoint.

    c# = -MinkDot#(a, b)

    ' Protect ACosh against roundoff.
    IF c# < 1# THEN c# = 1#

    distance# = ACosh#(c#)

    IF distance# < .000000000001# THEN EXIT SUB

    denominator# = HSinh#(distance#)

    ProjectPoint a, model%, oldX!, oldY!

    FOR i% = 1 TO EDGESEGS

        u# = i% / EDGESEGS

        wa# = HSinh#((1# - u#) * distance#) / denominator#
        wb# = HSinh#(u# * distance#) / denominator#

        DIM p AS HPoint

        p.x = wa# * a.x + wb# * b.x
        p.y = wa# * a.y + wb# * b.y
        p.t = wa# * a.t + wb# * b.t

        NormalizePoint p

        ProjectPoint p, model%, newX!, newY!

        LINE (oldX!, oldY!)-(newX!, newY!), col%

        oldX! = newX!
        oldY! = newY!

    NEXT i%

END SUB

SUB DrawEscherTesselation (maxTier%, model%, edgeLength#, tileCount%, truncated%)

    DIM a AS HPoint
    DIM b AS HPoint
    DIM apex AS HPoint
    DIM centre AS HPoint

    centre.x = 0#
    centre.y = 0#
    centre.t = 1#

    ' ------------------------------------------------------------
    ' Tier zero in the original program is peculiar:
    '
    '   r1 = 0
    '
    ' so all six "base" vertices collapse to the centre.
    ' The six triangle apices become every other vertex of
    ' the 12-point first ring.
    '
    ' Reproduce that connectivity explicitly.
    ' ------------------------------------------------------------

    FOR i% = 0 TO 5

        IF i% AND 1 THEN col% = 4 ELSE col% = 15

        RingPoint 1, 2 * i% + 1, edgeLength#, apex

        DrawEdge centre, apex, model%, col%

    NEXT i%


    ' ------------------------------------------------------------
    ' Subsequent tiers have precisely the connectivity of the
    ' original program.
    '
    ' Ring t:
    '
    '       N = 6 * 2^t
    '
    ' Each adjacent pair has an outward apex at vertex
    '
    '       2*i + 1
    '
    ' of the next ring, which has 2*N vertices.
    ' ------------------------------------------------------------

    tileCount% = 0
    truncated% = 0

    FOR tier% = 1 TO maxTier%

        n% = 6 * (2 ^ tier%)

        FOR i% = 0 TO n% - 1
            IF i% AND 1 THEN col% = 4 ELSE col% = 15

            j% = (i% + 1) MOD n%

            RingPoint tier%, i%, edgeLength#, a
            RingPoint tier%, j%, edgeLength#, b

            ' The circumferential base.
            DrawEdge a, b, model%, col%

            ' Exactly the same midpoint/doubling relationship
            ' as the original Euclidean shortcut.
            '
            ' The angular midpoint between vertices i and i+1
            ' becomes odd vertex 2*i+1 of the doubled ring.

            RingPoint tier% + 1, 2 * i% + 1, edgeLength#, apex

            DrawEdge a, apex, model%, col%
            DrawEdge b, apex, model%, col%

            tileCount% = tileCount% + 1

            IF tileCount% >= MAXTILES THEN truncated% = -1: EXIT SUB

        NEXT i%

    NEXT tier%

END SUB

' ================================================================
' Rendering geodesics
' ================================================================
SUB DrawPolygon (v() AS HPoint, model%, col%)

    FOR i% = 0 TO POLYSIDES - 1

        j% = (i% + 1) MOD POLYSIDES

        DrawEdge v(i%), v(j%), model%, col%

    NEXT i%

END SUB

SUB EdgeNormal (a AS HPoint, b AS HPoint, n AS HPoint)

    ' A hyperbolic geodesic in the hyperboloid model is the
    ' intersection of the hyperboloid with a plane through the
    ' origin.
    '
    ' Find the Minkowski-normal vector to the plane containing
    ' points a and b.

    n.x = a.t * b.y - a.y * b.t
    n.y = a.x * b.t - a.t * b.x
    n.t = a.x * b.y - a.y * b.x

    mag2# = MinkDot#(n, n)
    mag# = SQR(mag2#)

    n.x = n.x / mag#
    n.y = n.y / mag#
    n.t = n.t / mag#

END SUB

' ================================================================
' Construct the hyperbolic tessellation.
' ================================================================
SUB GenerateTiling (model%, tileCount%, truncated%)

    DIM first(0 TO POLYSIDES - 1) AS HPoint
    DIM cur(0 TO POLYSIDES - 1) AS HPoint

    DIM centre AS HPoint
    DIM childCentre AS HPoint
    DIM normal AS HPoint

    DIM top AS INTEGER
    DIM seenCount AS INTEGER
    DIM cameFrom AS INTEGER

    ' Maximum HYPERBOLIC distance of polygon centres from origin.
    maxDist# = 4.7#

    truncated% = 0
    tileCount% = 0

    ' A regular {p,q} polygon can be split from its centre into
    ' right hyperbolic triangles having angles:
    '
    '     pi/p, pi/q, pi/2
    '
    ' Its circumradius R therefore obeys
    '
    '     cosh(R) = cot(pi/p) cot(pi/q)

    ap# = PI / POLYSIDES
    aq# = PI / VERTEXQ

    cotp# = COS(ap#) / SIN(ap#)
    cotq# = COS(aq#) / SIN(aq#)

    coshR# = cotp# * cotq#

    IF coshR# <= 1# THEN
        LOCATE 3, 1
        PRINT "This {p,q} is not hyperbolic."
        EXIT SUB
    END IF

    sinhR# = SQR(coshR# * coshR# - 1#)

    ' ------------------------------------------------------------
    ' Construct the FIRST polygon in hyperbolic space.
    '
    ' A point at hyperbolic radius R and angle theta is:
    '
    '   X = sinh(R) cos(theta)
    '   Y = sinh(R) sin(theta)
    '   T = cosh(R)
    '
    ' No disk coordinates are involved here.
    ' ------------------------------------------------------------

    FOR i% = 0 TO POLYSIDES - 1

        theta# = 2# * PI * i% / POLYSIDES

        first(i%).x = sinhR# * COS(theta#)
        first(i%).y = sinhR# * SIN(theta#)
        first(i%).t = coshR#

    NEXT i%

    centre.x = 0#
    centre.y = 0#
    centre.t = 1#

    ' Initial polygon goes onto our traversal stack.

    top = 0

    FOR i% = 0 TO POLYSIDES - 1
        CopyPoint first(i%), stackV(top, i%)
    NEXT i%

    CopyPoint centre, stackC(top)

    stackSkip(top) = -1
    top = top + 1

    ' Initial centre is (0,0) in either disk.
    seenCount = 1
    seenX(0) = 0#
    seenY(0) = 0#

    ' ------------------------------------------------------------
    ' Traverse the tiling.
    '
    ' Reflect each polygon across each of its edges.  Reflection
    ' produces the genuine adjacent regular hyperbolic polygon.
    ' ------------------------------------------------------------

    DO WHILE top > 0

        top = top - 1

        FOR i% = 0 TO POLYSIDES - 1
            CopyPoint stackV(top, i%), cur(i%)
        NEXT i%

        CopyPoint stackC(top), centre

        cameFrom = stackSkip(top)

        tileCount% = tileCount% + 1

        DrawPolygon cur(), model%, 15

        FOR edge% = 0 TO POLYSIDES - 1

            ' Don't immediately reflect back into our parent.
            IF edge% <> cameFrom THEN

                nextEdge% = (edge% + 1) MOD POLYSIDES

                EdgeNormal cur(edge%), cur(nextEdge%), normal

                ' Reflect the polygon CENTRE first.  That lets us
                ' cheaply decide whether this neighbour is new and
                ' within our requested hyperbolic radius.

                ReflectPoint centre, normal, childCentre

                dist# = ACosh#(childCentre.t)

                IF dist# <= maxDist# THEN

                    IF NOT AlreadySeen%(childCentre, seenCount) THEN

                        IF seenCount >= MAXTILES OR top >= MAXSTACK THEN
                            truncated% = -1

                        ELSE
                            ' Record the centre in Poincare
                            ' coordinates solely for duplicate
                            ' detection.

                            seenX(seenCount) = childCentre.x / (childCentre.t + 1#)
                            seenY(seenCount) = childCentre.y / (childCentre.t + 1#)
                            seenCount = seenCount + 1

                            ' Now perform the same genuine hyperbolic
                            ' reflection on every vertex.

                            FOR j% = 0 TO POLYSIDES - 1
                                ReflectPoint cur(j%), normal, stackV(top, j%)
                            NEXT j%

                            CopyPoint childCentre, stackC(top)

                            ' Reflection leaves this shared edge
                            ' fixed, so the same edge index leads
                            ' straight back again.
                            stackSkip(top) = edge%

                            top = top + 1
                        END IF
                    END IF
                END IF
            END IF

        NEXT edge%

    LOOP

END SUB

FUNCTION HSinh# (x#)

    HSinh# = (EXP(x#) - EXP(-x#)) / 2#

END FUNCTION

' ================================================================
' Minkowski / hyperboloid geometry
' ================================================================
FUNCTION MinkDot# (a AS HPoint, b AS HPoint)

    MinkDot# = a.x * b.x + a.y * b.y - a.t * b.t

END FUNCTION

SUB NormalizePoint (p AS HPoint)

    ' A point on the upper hyperboloid satisfies:
    '
    '     t^2 - x^2 - y^2 = 1
    '
    ' Reflection already preserves this mathematically.  Rebuilding
    ' t merely removes accumulated floating-point drift.

    p.t = SQR(1# + p.x * p.x + p.y * p.y)

END SUB

' ================================================================
' Projection
' ================================================================
SUB ProjectPoint (p AS HPoint, model%, sx!, sy!)

    IF model% = POINCARE THEN

        ' Stereographic projection onto the Poincare disk:
        '
        '       x = X / (T + 1)
        '       y = Y / (T + 1)

        dx# = p.x / (p.t + 1#)
        dy# = p.y / (p.t + 1#)

    ELSE

        ' Central projection onto the Klein disk:
        '
        '       x = X / T
        '       y = Y / T

        dx# = p.x / p.t
        dy# = p.y / p.t

    END IF

    sx! = scrCX + scrR * dx#

    ' Screen Y coordinates run downward.
    sy! = scrCY - scrR * dy#

END SUB

SUB ReflectPoint (p AS HPoint, n AS HPoint, r AS HPoint)

    ' Reflection across a geodesic:
    '
    '     r = p - 2 <p,n> n
    '
    ' where <,> is the Minkowski inner product and n has
    ' Minkowski norm +1.

    d# = MinkDot#(p, n)

    r.x = p.x - 2# * d# * n.x
    r.y = p.y - 2# * d# * n.y
    r.t = p.t - 2# * d# * n.t

    NormalizePoint r

END SUB

SUB RingPoint (tier%, index%, edgeLength#, p AS HPoint)

    ' The original construction has
    '
    '     N = 6 * 2^tier
    '
    ' points on each ring.

    n% = 6 * (2 ^ tier%)

    theta# = 2# * PI * index% / n%

    ' Choose the hyperbolic radius so that adjacent points
    ' on this ring are exactly edgeLength apart.
    '
    '   sinh(rho) =
    '
    '       sinh(edgeLength / 2)
    '       --------------------
    '           sin(pi / N)

    sinhR# = HSinh#(edgeLength# / 2#) / SIN(PI / n%)

    coshR# = SQR(1# + sinhR# * sinhR#)

    ' Actual point in the hyperboloid model.

    p.x = sinhR# * COS(theta#)
    p.y = sinhR# * SIN(theta#)
    p.t = coshR#

END SUB
