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

TYPE UVPoint
    u AS DOUBLE
    v AS DOUBLE
END TYPE

CONST PI = 3.14159265358979#
CONST TILINGPOLYSIDES = 3
CONST TILINGVERTEXQ = 7

CONST KLEIN = 0
CONST POINCARE = 1

CONST TILING = 0
CONST SUBDIVISION = 1
CONST DEMONS = 2

' Enough for the default {3,7} example and MAXDIST = 4.7.
CONST MAXTILES = 1600
CONST MAXSTACK = 256

' Number of straight screen segments used to approximate a
' projected Poincare geodesic.
CONST EDGESEGS = 12

' Demons and Angels Parameters
CONST DEMONPOLYSIDES = 6
CONST DEMONVERTEXQ = 4

CONST MAXPOLYSIDES = (TILINGPOLYSIDES + DEMONPOLYSIDES + (TILINGPOLYSIDES + DEMONPOLYSIDES) ^ 2 ^ .5) / 2

' Maximum number of points in the demon outline. In case it gets switched out.
CONST MAXDEMONPOINTS = 200

DECLARE FUNCTION ACosh# (x#)
DECLARE FUNCTION HCosh# (x#)
DECLARE FUNCTION HSinh# (x#)
DECLARE FUNCTION MinkDot# (a AS HPoint, b AS HPoint)
DECLARE FUNCTION AlreadySeen% (p AS HPoint, count%)

DECLARE SUB CopyPoint (a AS HPoint, b AS HPoint)
DECLARE SUB NormalizePoint (p AS HPoint)
DECLARE SUB GenerateTiling (polysides%, vertexq%, model%, figure%, tileCount%, truncated%)
DECLARE SUB EdgeNormal (a AS HPoint, b AS HPoint, n AS HPoint)
DECLARE SUB ReflectPoint (p AS HPoint, n AS HPoint, r AS HPoint)

DECLARE SUB ProjectPoint (p AS HPoint, model%, sx!, sy!)
DECLARE SUB DrawEdge (a AS HPoint, b AS HPoint, model%, col%)
DECLARE SUB DrawPolygon (polysides%, v() AS HPoint, model%, col%)
DECLARE SUB RingPoint (tier%, index%, edgeLength#, p AS HPoint)
DECLARE SUB DrawEscherTesselation (maxTier%, model%, edgeLength#, tileCount%, truncated%)

DECLARE SUB LoadDemon ()
DECLARE SUB BuildSectorFrame (c AS HPoint, a AS HPoint, b AS HPoint, f AS HPoint, s AS HPoint)
DECLARE SUB ExpAtFrame (u#, v#, c AS HPoint, f AS HPoint, s AS HPoint, p AS HPoint)
DECLARE SUB NormalizeSpaceVector (v AS HPoint)
DECLARE SUB DrawEscherHexagon (v() AS HPoint, c AS HPoint, parity%, model%, col%)
DECLARE SUB DrawDemonSector (c AS HPoint, a AS HPoint, b AS HPoint, model%, col%)

DIM SHARED scrCX AS INTEGER
DIM SHARED scrCY AS INTEGER
DIM SHARED scrR AS INTEGER

' Explicit traversal stack.  We need retain only polygons which
' have not yet been processed.
DIM SHARED stackV(0 TO MAXSTACK - 1, 0 TO MAXPOLYSIDES - 1) AS HPoint
DIM SHARED stackC(0 TO MAXSTACK - 1) AS HPoint
DIM SHARED stackSkip(0 TO MAXSTACK - 1) AS INTEGER
DIM SHARED stackParity(0 TO MAXSTACK - 1) AS INTEGER

' To recognize polygons reached by two different paths, storing
' only their centre in Poincare coordinates is sufficient.
DIM SHARED seenX(0 TO MAXTILES - 1) AS DOUBLE
DIM SHARED seenY(0 TO MAXTILES - 1) AS DOUBLE

' Demons and Angels!
DIM SHARED Demon(MAXDEMONPOINTS) AS UVPoint
DIM SHARED DemonCount AS INTEGER

' ------------------------------------------------
' Main program
' ------------------------------------------------

LoadDemon

SCREEN 12

scrCX = 320
scrCY = 240
scrR = 220

model% = KLEIN
figure% = TILING

maxTier% = 7

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
            PRINT "Tiling: {"; LTRIM$(STR$(TILINGPOLYSIDES)); ","; LTRIM$(STR$(TILINGVERTEXQ)); "}"
            GenerateTiling TILINGPOLYSIDES, TILINGVERTEXQ, model%, figure%, tileCount%, truncated%
        CASE SUBDIVISION:
            PRINT "Escher tesselation: "; LTRIM$(STR$(maxTier%)); " tiers with edge length "; LTRIM$(STR$(edgeLength#)); "}"
            DrawEscherTesselation maxTier%, model%, edgeLength#, tileCount%, truncated%
        CASE DEMONS:
            PRINT "Demons and Angels"

            ' Lay a white background for the angels,
            ' to carve the black demons out of
            CIRCLE (scrCX, scrCY), scrR, 8
            PAINT (scrCX, scrCY), 15, 8

            GenerateTiling DEMONPOLYSIDES, DEMONVERTEXQ, model%, figure%, tileCount%, truncated%
    END SELECT

    ' Boundary at hyperbolic infinity.
    CIRCLE (scrCX, scrCY), scrR, 8

    LOCATE 29, 1
    PRINT LTRIM$(STR$(tileCount%)); " polygons";
    LOCATE 30, 1
    PRINT " [ T=Tiling, S=Tesselation, D=Demons | P=Poincare, K=Klein ]  Q=quit";

    IF truncated% THEN LOCATE 28, 1: PRINT "[TRUNCATED]";

    DO
        k$ = UCASE$(INKEY$)
    LOOP WHILE k$ = ""

    SELECT CASE k$
        CASE "T": figure% = TILING
        CASE "S": figure% = SUBDIVISION
        CASE "D": figure% = DEMONS

        CASE "P": model% = POINCARE
        CASE "K": model% = KLEIN

        CASE "Q", CHR$(27): EXIT DO
    END SELECT
LOOP

SCREEN 0
END

DemonData:
DATA 92
DATA .867909932,0,.882128971,.043955949,.918992066,.070826638,1.02653730,.087006719
DATA 1.09393406,.107128876,1.09393406,.107128876,1.09393406,.107128495,1.07265040,.126092059
DATA 1.03059491,.136494138,.940475521,.128723795,.863264163,.097637657,.774522004,.090559706
DATA .777606462,.161032866,.802801818,.208218255,.846983206,.248491164,.890492253,.294041845
DATA .976537195,.487818109,.992652031,.573107917,.902312644,.468357222,.802465087,.395433240
DATA .692318044,.362756431,.571079858,.348961308,.517664962,.298250384,.450670705,.258633549
DATA .381620590,.248728268,.322039359,.257365054,.338018717,.175474294,.347469642,.098191218
DATA .333860086,.082436066,.307474946,.076365057,.285027317,.080695364,.283235907,.096143782
DATA .174579461,.066665854,.155698938,.073775832,.152175607,.090770121,.166364340,.140366680
DATA .167484533,.187366198,.167265658,.187366198,.167046782,.187366198,.166827907,.187366198
DATA .166609032,.187366198,.166390156,.187366198,.166171281,.187366198,.096018933,.120717778
DATA .080561845,.066059249,.053099165,.025149804,0,0,.053099165,-.02514980
DATA .080561845,-.06605925,.096018933,-.12071778,.166171281,-.18736620,.166390156,-.18736620
DATA .166609032,-.18736620,.166827907,-.18736620,.167046782,-.18736620,.167265658,-.18736620
DATA .167484533,-.18736620,.166364340,-.14036668,.152175607,-.09077012,.155698938,-.07377583
DATA .174579461,-.06666585,.283235907,-.09614378,.285027317,-.08069536,.307474946,-.07636506
DATA .333860086,-.08243607,.347469642,-.09819122,.338018717,-.17547429,.322039359,-.25736505
DATA .381620590,-.24872827,.450670705,-.25863355,.517664962,-.29825038,.571079858,-.34896131
DATA .692318044,-.36275643,.802465087,-.39543324,.902312644,-.46835722,.992652031,-.57310792
DATA .976537195,-.48781811,.890492253,-.29404185,.846983206,-.24849116,.802801818,-.20821825
DATA .777606462,-.16103287,.774522004,-.09055971,.863264163,-.09763766,.940475521,-.12872380
DATA 1.03059491,-.13649414,1.07265040,-.12609206,1.09393406,-.10712849,1.09393406,-.10712888
DATA 1.09393406,-.10712888,1.02653730,-.08700672,.918992066,-.07082664,.882128971,-.04395595


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

SUB BuildSectorFrame (c AS HPoint, a AS HPoint, b AS HPoint, f AS HPoint, s AS HPoint)

    DIM r AS HPoint
    DIM hint AS HPoint

    ' -----------------------------------------------------------
    ' Forward direction:
    '
    ' A+B points toward the midpoint of edge AB. Project it into
    ' the tangent plane at C.
    '
    ' For <C,C> = -1:
    '
    '     tangent(V) = V + <C,V>C
    ' -----------------------------------------------------------

    r.x = a.x + b.x
    r.y = a.y + b.y
    r.t = a.t + b.t

    d# = MinkDot#(c, r)

    f.x = r.x + d# * c.x
    f.y = r.y + d# * c.y
    f.t = r.t + d# * c.t

    NormalizeSpaceVector f


    ' -----------------------------------------------------------
    ' Sideways direction.
    '
    ' Construct a vector Minkowski-orthogonal to both C and F.
    '
    ' This is the same Lorentz-cross-product construction used for
    ' the normal of a hyperbolic geodesic.
    ' -----------------------------------------------------------

    s.x = c.t * f.y - c.y * f.t
    s.y = c.x * f.t - c.t * f.x
    s.t = c.x * f.y - c.y * f.x

    NormalizeSpaceVector s


    ' -----------------------------------------------------------
    ' Choose the orientation of S from the actual edge ordering.
    '
    ' A-B points from B toward A. Project that into C's tangent
    ' plane and make S point to the same side.
    '
    ' This is important because reflection reverses polygon
    ' orientation, and we WANT the demon to be reflected with it.
    ' -----------------------------------------------------------

    hint.x = a.x - b.x
    hint.y = a.y - b.y
    hint.t = a.t - b.t

    d# = MinkDot#(c, hint)

    hint.x = hint.x + d# * c.x
    hint.y = hint.y + d# * c.y
    hint.t = hint.t + d# * c.t

    IF MinkDot#(s, hint) < 0# THEN
        s.x = -s.x
        s.y = -s.y
        s.t = -s.t
    END IF

END SUB

SUB CopyPoint (a AS HPoint, b AS HPoint)

    b.x = a.x
    b.y = a.y
    b.t = a.t

END SUB

SUB DrawDemonSector (c AS HPoint, a AS HPoint, b AS HPoint, model%, col%)

    DIM forward AS HPoint
    DIM sideways AS HPoint
    DIM p1 AS HPoint
    DIM p2 AS HPoint
    DIM centroid AS UVPoint

    BuildSectorFrame c, a, b, forward, sideways

    ExpAtFrame Demon(0).u, Demon(0).v, c, forward, sideways, p2

    FOR i% = 1 TO DemonCount

        IF i% = DemonCount THEN j% = 0 ELSE j% = i%

        p1 = p2
        ExpAtFrame Demon(j%).u, Demon(j%).v, c, forward, sideways, p2

        ' This is the existing projection-aware geodesic renderer:
        ' straight in Klein, curved in Poincare.
        DrawEdge p1, p2, model%, col%

        ' Sum up the points to average them to find the centroid
        centroid.u = centroid.u + Demon(j%).u
        centroid.v = centroid.v + Demon(j%).v

    NEXT i%

    ' Finish up centroid calculation
    centroid.u = centroid.u / DemonCount
    centroid.v = centroid.v / DemonCount

    ' Fill the demon
    ExpAtFrame centroid.u, centroid.v, c, forward, sideways, p1

    ProjectPoint p1, model%, x!, y!

    PAINT (x!, y!), col%, col%

END SUB

' ================================================================
' Rendering geodesics
' ================================================================
SUB DrawEdge (a AS HPoint, b AS HPoint, model%, col%)

    IF model% = KLEIN THEN

        ' In the Klein model a hyperbolic geodesic projects EXACTLY
        ' to a straight Euclidean chord.

        ProjectPoint a, model%, x1!, y1!
        ProjectPoint b, model%, x2!, y2!

        LINE (x1!, y1!)-(x2!, y2!), col%

    ELSE

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

    END IF

END SUB

SUB DrawEscherHexagon (v() AS HPoint, c AS HPoint, parity%, model%, col%)

    ' The six triangular sectors around a {6,4} hexagon alternate:
    '
    '     demon, angel, demon, angel, demon, angel
    '
    ' Only demons are explicitly drawn.  If their outline is fitted
    ' correctly, the angels are exactly the negative space.
    '
    ' parity toggles on every move to an adjacent hexagon.

    FOR edge% = 0 TO 5

        IF (edge% + parity%) MOD 2 = 0 THEN

            nextEdge% = edge% + 1
            IF nextEdge% = 6 THEN nextEdge% = 0

            DrawDemonSector c, v(edge%), v(nextEdge%), model%, col%

        END IF

    NEXT edge%

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
SUB DrawPolygon (polysides%, v() AS HPoint, model%, col%)

    FOR i% = 0 TO polysides% - 1

        j% = (i% + 1) MOD polysides%

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

SUB ExpAtFrame (u#, v#, c AS HPoint, f AS HPoint, s AS HPoint, p AS HPoint)

    ' Exponential map from the ordinary 2-D tangent plane at C onto
    ' the hyperboloid:
    '
    ' P = cosh(r) C + sinh(r) (u F + v S) / r
    '
    ' where r = sqrt(u^2 + v^2).
    '
    ' Thus the prototype path itself stays "pristine": its ordinary
    ' Euclidean coordinates are interpreted as tangent vectors, and
    ' this routine turns them into genuine hyperbolic points.

    r# = SQR(u# * u# + v# * v#)

    IF r# < .000000000001# THEN
        CopyPoint c, p
        EXIT SUB
    END IF

    ch# = HCosh#(r#)
    shOverR# = HSinh#(r#) / r#

    p.x = ch# * c.x + shOverR# * (u# * f.x + v# * s.x)
    p.y = ch# * c.y + shOverR# * (u# * f.y + v# * s.y)
    p.t = ch# * c.t + shOverR# * (u# * f.t + v# * s.t)

    ' The equation above preserves the hyperboloid exactly in
    ' mathematics; this only removes floating-point drift.
    NormalizePoint p

END SUB

' ================================================================
' Construct the hyperbolic tessellation.
' ================================================================
SUB GenerateTiling (polysides%, vertexq%, model%, figure%, tileCount%, truncated%)

    DIM first(0 TO polysides% - 1) AS HPoint
    DIM cur(0 TO polysides% - 1) AS HPoint

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

    ap# = PI / polysides%
    aq# = PI / vertexq%

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

    FOR i% = 0 TO polysides% - 1

        theta# = 2# * PI * i% / polysides%

        first(i%).x = sinhR# * COS(theta#)
        first(i%).y = sinhR# * SIN(theta#)
        first(i%).t = coshR#

    NEXT i%

    centre.x = 0#
    centre.y = 0#
    centre.t = 1#

    ' Initial polygon goes onto our traversal stack.

    top = 0

    FOR i% = 0 TO polysides% - 1
        CopyPoint first(i%), stackV(top, i%)
    NEXT i%

    CopyPoint centre, stackC(top)

    stackSkip(top) = -1
    stackParity(top) = 1
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

        FOR i% = 0 TO polysides% - 1
            CopyPoint stackV(top, i%), cur(i%)
        NEXT i%

        CopyPoint stackC(top), centre

        cameFrom = stackSkip(top)
        tileParity% = stackParity(top)

        tileCount% = tileCount% + 1

        SELECT CASE figure%
            CASE TILING: DrawPolygon polysides%, cur(), model%, 15
            CASE DEMONS: DrawEscherHexagon cur(), centre, tileParity%, model%, 0
        END SELECT

        FOR edge% = 0 TO polysides% - 1

            ' Don't immediately reflect back into our parent.
            IF edge% <> cameFrom THEN

                nextEdge% = (edge% + 1) MOD polysides%

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

                            FOR j% = 0 TO polysides% - 1
                                ReflectPoint cur(j%), normal, stackV(top, j%)
                            NEXT j%

                            CopyPoint childCentre, stackC(top)

                            ' Reflection leaves this shared edge
                            ' fixed, so the same edge index leads
                            ' straight back again.
                            stackSkip(top) = edge%
                            stackParity(top) = 1 - tileParity%

                            top = top + 1
                        END IF
                    END IF
                END IF
            END IF

        NEXT edge%

    LOOP

END SUB

FUNCTION HCosh# (x#)

    HCosh# = (EXP(x#) + EXP(-x#)) / 2#

END FUNCTION

FUNCTION HSinh# (x#)

    HSinh# = (EXP(x#) - EXP(-x#)) / 2#

END FUNCTION

SUB LoadDemon

    RESTORE DemonData

    READ DemonCount

    IF DemonCount < 3 OR DemonCount > MAXDEMONPOINTS THEN
        PRINT "Bad demon point count:"; DemonCount
        END
    END IF

    FOR i% = 0 TO DemonCount - 1
        READ Demon(i%).u, Demon(i%).v
    NEXT i%

END SUB

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

SUB NormalizeSpaceVector (v AS HPoint)

    ' A tangent vector in the hyperboloid model is spacelike:
    '
    '     <v,v> > 0
    '
    ' Normalize it to Minkowski length +1.

    m2# = MinkDot#(v, v)

    IF m2# <= 0# THEN
        PRINT "Bad tangent vector"
        PRINT "X ="; v.x
        PRINT "Y ="; v.y
        PRINT "T ="; v.t
        PRINT "<V,V> ="; m2#
        END
    END IF

    m# = SQR(m2#)

    v.x = v.x / m#
    v.y = v.y / m#
    v.t = v.t / m#

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
