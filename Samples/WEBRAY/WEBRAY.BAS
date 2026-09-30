' WEBRAY - a SCREEN 7 texture-mapped raycaster for WebQB / QuickBASIC
' Uses the original shareware map and directional wall art on two EGA pages.

DEFINT A-Z
'$DYNAMIC

DECLARE SUB LoadColumns ()
DECLARE SUB BuildLevel ()
DECLARE SUB AddObject (kind%, pic%, x!, y!, hp%)
DECLARE SUB RenderWorld ()
DECLARE SUB WallColumn (sx%, top%, bot%, wall%, tx%, side%, projected%)
DECLARE SUB RenderSprites ()
DECLARE SUB RenderSprite (n%)
DECLARE SUB DrawHUD ()
DECLARE SUB ReadControls ()
DECLARE SUB TurnPlayer (amount!)
DECLARE SUB TryMove (dx!, dy!)
DECLARE SUB FireWand ()
DECLARE SUB UpdateWand ()
DECLARE SUB ResolveShot ()
DECLARE SUB UpdateMonsters ()
DECLARE SUB CollectObject (n%)
DECLARE SUB DamageEnemy (n%, damage%)
DECLARE FUNCTION OpenTile% (tx%, ty%)
DECLARE FUNCTION InfoHigh% (value%)
DECLARE FUNCTION WallAt% (x!, y!)
DECLARE FUNCTION DimColor% (c%)

DIM SHARED world%(0 TO 1119)
DIM SHARED info%(0 TO 1119)
DIM SHARED texpack%(0 TO 24575)
DIM SHARED sprpack%(0 TO 18431)
DIM SHARED lighttex%(0 TO 70), darktex%(0 TO 70)
DIM SHARED hud%(0 TO 6401)
DIM SHARED hudart%(0 TO 3367)
DIM SHARED handpic%(0 TO 1141), handmask%(0 TO 1141)
DIM SHARED zbuffer!(0 TO 159)
DIM SHARED sprx!(1 TO 96), spry!(1 TO 96)
DIM SHARED sprdx!(1 TO 96), sprdy!(1 TO 96)
DIM SHARED sprkind%(1 TO 96), sprpic%(1 TO 96), spralive%(1 TO 96)
DIM SHARED sprhp%(1 TO 96), sprcool%(1 TO 96)
DIM SHARED px!, py!, dx!, dy!, planex!, planey!
DIM SHARED workpage%, visiblepage%, frame%, quitflag%
DIM SHARED health%, score%, shotflash%, hurtflash%, monsters%, outcome%, newobject%
DIM SHARED redkey%, greenkey%, bluekey%, bolts%, treasures%
DIM SHARED yellowkey%, potions%, scrolls%, purplegem%
DIM SHARED wandphase%, wandstep%, handy%
DIM SHARED messageframes%
DIM SHARED skycolor%

SCREEN 7
LoadColumns
BuildLevel

workpage% = 1
visiblepage% = 0
DO
    ReadControls
    UpdateMonsters
    RenderWorld
LOOP UNTIL quitflag%

SCREEN 7, , 0, 0
CLS
COLOR 15
IF outcome% = 1 THEN
    LOCATE 10, 12: PRINT "THE GATE OPENS!"
    LOCATE 12, 8: PRINT "TOWNE CEMETARY IS CLEARED."
ELSEIF outcome% = 2 THEN
    LOCATE 11, 12: PRINT "THE ABYSS CLAIMS YOU."
ELSE
    LOCATE 11, 14: PRINT "THE DUNGEON RESTS."
END IF
LOCATE 13, 12: PRINT "FINAL SCORE:"; score%
LOCATE 16, 10: PRINT "PRESS A KEY TO RETURN"
DO: k$ = INKEY$: LOOP WHILE k$ = ""
SCREEN 0
END

SUB BuildLevel
    DEF SEG = VARSEG(world%(0)): BLOAD "CATMAP.BSV", VARPTR(world%(0))
    DEF SEG = VARSEG(info%(0)): BLOAD "CATINFO.BSV", VARPTR(info%(0))
    DEF SEG
    ' Plane zero also contains non-wall metadata. Strip it once so the hot
    ' DDA loop only needs a single zero/nonzero array test per crossed cell.
    FOR i% = 0 TO 1119
        IF world%(i%) > 70 THEN world%(i%) = 0
    NEXT i%
    RESTORE LightFaces
    FOR i% = 0 TO 70: READ lighttex%(i%): NEXT i%
    RESTORE DarkFaces
    FOR i% = 0 TO 70: READ darktex%(i%): NEXT i%

    health% = 100: score% = 0: frame% = 0
    quitflag% = 0: shotflash% = 0: hurtflash% = 0
    outcome% = 0: redkey% = 0: greenkey% = 0: bluekey% = 0
    bolts% = 0: treasures% = 0
    potions% = 0: scrolls% = 0: purplegem% = 0: yellowkey% = 0
    wandphase% = 0: wandstep% = 0: handy% = 120

    monsters% = 0
    FOR i% = 0 TO 1119
        code% = info%(i%) AND 255
        IF code% > 0 THEN
            x% = i% MOD 40: y% = i% \ 40
            SELECT CASE code%
                CASE 1 TO 4
                    px! = x% + .5: py! = y% + .5
                    IF code% = 1 THEN dx! = 0: dy! = -1
                    IF code% = 2 THEN dx! = 1: dy! = 0
                    IF code% = 3 THEN dx! = 0: dy! = 1
                    IF code% = 4 THEN dx! = -1: dy! = 0
                    planex! = -dy! * .66: planey! = dx! * .66
                CASE 5: AddObject 3, 6, x% + .5, y% + .5, 1
                CASE 7: AddObject 4, 7, x% + .5, y% + .5, 1
                CASE 8: AddObject 5, 8, x% + .5, y% + .5, 1
                CASE 10: AddObject 6, 9, x% + .5, y% + .5, 1
                CASE 11: AddObject 7, 10, x% + .5, y% + .5, 1
                CASE 12 TO 19: AddObject 9, 12, x% + .5, y% + .5, 1
                CASE 21: AddObject 10, 13, x% + .5, y% + .5, 1
                CASE 25, 38, 43: AddObject 2, 4, x% + .5, y% + .5, 1
                CASE 46 TO 48: AddObject 11, code% - 32, x% + .5, y% + .5, 99
                CASE 52, 53: AddObject 1, 1, x% + .5, y% + .5, 3
                CASE 58 TO 62: AddObject 8, 11, x% + .5, y% + .5, 1
            END SELECT
        END IF
    NEXT i%

LightFaces:
    DATA 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0
    DATA 2,0,0,0,0,0,0,0,0,0,0,0,0,0,4,0,0,0,0,0,6,8,10,0,12
    DATA 0,0,0,14,0,16,0,18,20,0,22,0,0,0,0,0,0,0,0,0,0
DarkFaces:
    DATA 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1
    DATA 3,0,0,0,0,0,0,0,0,0,0,0,0,0,5,0,0,0,0,0,7,9,11,0,13
    DATA 0,0,0,15,0,17,0,19,21,0,23,0,0,0,0,0,0,0,0,0,0
END SUB

SUB LoadColumns
    DEF SEG = VARSEG(texpack%(0)): BLOAD "CATPACK.BSV", VARPTR(texpack%(0))
    DEF SEG = VARSEG(sprpack%(0)): BLOAD "CATSPR.BSV", VARPTR(sprpack%(0))
    DEF SEG = VARSEG(hud%(0)): BLOAD "HUD.BSV", VARPTR(hud%(0))
    DEF SEG = VARSEG(hudart%(0)): BLOAD "HUDART.BSV", VARPTR(hudart%(0))
    DEF SEG = VARSEG(handpic%(0)): BLOAD "HAND.BSV", VARPTR(handpic%(0))
    DEF SEG = VARSEG(handmask%(0)): BLOAD "HANDMASK.BSV", VARPTR(handmask%(0))
    DEF SEG
END SUB

SUB AddObject (kind%, pic%, x!, y!, hp%)
    slot% = 0
    FOR i% = 1 TO monsters%
        IF spralive%(i%) = 0 AND slot% = 0 THEN slot% = i%
    NEXT i%
    IF slot% = 0 THEN
        IF monsters% >= 96 THEN newobject% = 0: EXIT SUB
        monsters% = monsters% + 1: slot% = monsters%
    END IF
    sprkind%(slot%) = kind%: sprpic%(slot%) = pic%
    sprx!(slot%) = x!: spry!(slot%) = y!
    sprdx!(slot%) = 0: sprdy!(slot%) = 0
    sprhp%(slot%) = hp%: sprcool%(slot%) = 0
    spralive%(slot%) = -1: newobject% = slot%
END SUB

SUB RenderWorld
    SCREEN 7, , workpage%, visiblepage%
    CLS

    sky% = 1
    IF hurtflash% > 0 THEN sky% = 4
    skycolor% = sky%
    ' Catacomb Abyss uses a 320x120 view over an 80-pixel status panel.
    LINE (0, 0)-(319, 59), sky%, BF
    LINE (0, 60)-(319, 119), 8, BF

    rayx! = dx! - planex!: rayy! = dy! - planey!
    raystepx! = planex! * 2! / 159!: raystepy! = planey! * 2! / 159!
    FOR ray% = 0 TO 159
        mapx% = INT(px!): mapy% = INT(py!)
        mappos% = mapy% * 40 + mapx%

        IF ABS(rayx!) < .0001 THEN
            ddx! = 10000!
        ELSE
            ddx! = ABS(1! / rayx!)
        END IF
        IF ABS(rayy!) < .0001 THEN
            ddy! = 10000!
        ELSE
            ddy! = ABS(1! / rayy!)
        END IF

        IF rayx! < 0 THEN
            stepx% = -1: sdx! = (px! - mapx%) * ddx!
        ELSE
            stepx% = 1: sdx! = (mapx% + 1! - px!) * ddx!
        END IF
        IF rayy! < 0 THEN
            stepy% = -1: sdy! = (py! - mapy%) * ddy!
        ELSE
            stepy% = 1: sdy! = (mapy% + 1! - py!) * ddy!
        END IF

        side% = 0
        DO
            IF sdx! < sdy! THEN
                sdx! = sdx! + ddx!: mapx% = mapx% + stepx%: mappos% = mappos% + stepx%: side% = 0
            ELSE
                sdy! = sdy! + ddy!: mapy% = mapy% + stepy%: mappos% = mappos% + stepy% * 40: side% = 1
            END IF
            wall% = world%(mappos%)
        LOOP WHILE wall% = 0

        IF side% = 0 THEN
            dist! = (mapx% - px! + (1 - stepx%) / 2!) / rayx!
            wallx! = py! + dist! * rayy!
        ELSE
            dist! = (mapy% - py! + (1 - stepy%) / 2!) / rayy!
            wallx! = px! + dist! * rayx!
        END IF
        IF dist! < .05 THEN dist! = .05
        wallx! = wallx! - INT(wallx!)
        texx% = INT(wallx! * 64!)
        IF side% = 0 AND rayx! < 0 THEN texx% = 63 - texx%
        IF side% = 1 AND rayy! > 0 THEN texx% = 63 - texx%

        height% = INT(100! / dist!)
        top% = 60 - height% \ 2
        bot% = 60 + height% \ 2
        IF top% < 0 THEN top% = 0
        IF bot% > 119 THEN bot% = 119
        zbuffer!(ray%) = dist!
        IF side% = 0 THEN texture% = lighttex%(wall%) ELSE texture% = darktex%(wall%)
        WallColumn ray% * 2, top%, bot%, texture%, texx%, side%, height%
        rayx! = rayx! + raystepx!: rayy! = rayy! + raystepy!
    NEXT ray%

    RenderSprites
    UpdateWand
    IF messageframes% > 0 THEN
        LINE (67, 45)-(252, 73), 7, BF
        LINE (67, 45)-(252, 73), 15, B
        COLOR 0, 7: LOCATE 8, 11: PRINT "THE DOOR IS BLOCKED";
        COLOR 15, 0
        messageframes% = messageframes% - 1
    END IF
    DrawHUD

    SCREEN 7, , workpage%, workpage%
    visiblepage% = workpage%
    workpage% = 1 - workpage%
    frame% = frame% + 1
    IF shotflash% > 0 THEN shotflash% = shotflash% - 1
    IF hurtflash% > 0 THEN hurtflash% = hurtflash% - 1
END SUB

SUB WallColumn (sx%, top%, bot%, texture%, tx%, side%, projected%)
    h& = projected%
    IF h& < 1 THEN EXIT SUB
    ybase& = 60& - h& \ 2&
    first& = ((top% - ybase&) * 64&) \ h&
    last& = ((bot% - ybase&) * 64&) \ h&
    IF first& < 0 THEN first& = 0
    IF last& > 63 THEN last& = 63
    texbase& = texture% * 1024& + tx% \ 4
    nibble% = tx% AND 3
    IF nibble% = 0 THEN mask% = 15: divisor% = 1
    IF nibble% = 1 THEN mask% = 240: divisor% = 16
    IF nibble% = 2 THEN mask% = 3840: divisor% = 256
    IF h& < 64& THEN
        screenlast& = ybase& + ((last& + 1&) * h&) \ 64& - 1&
        IF screenlast& > bot% THEN screenlast& = bot%
        IF nibble% < 3 THEN
            FOR sy& = top% TO screenlast&
                ty& = (((sy& - ybase& + 1&) * 64&) - 1&) \ h&
                word% = texpack%(texbase& + ty& * 16&)
                c% = (word% AND mask%) \ divisor%
                LINE (sx%, sy&)-(sx% + 1, sy&), c%, BF
            NEXT sy&
        ELSE
            FOR sy& = top% TO screenlast&
                ty& = (((sy& - ybase& + 1&) * 64&) - 1&) \ h&
                word% = texpack%(texbase& + ty& * 16&)
                c% = (word% AND 28672) \ 4096
                IF word% < 0 THEN c% = c% + 8
                LINE (sx%, sy&)-(sx% + 1, sy&), c%, BF
            NEXT sy&
        END IF
        EXIT SUB
    END IF
    IF nibble% < 3 THEN
        FOR ty& = first& TO last&
            y1& = ybase& + (ty& * h&) \ 64&: y2& = ybase& + ((ty& + 1&) * h&) \ 64& - 1&
            IF y1& < top% THEN y1& = top%
            IF y2& > bot% THEN y2& = bot%
            IF y2& >= y1& THEN
                word% = texpack%(texbase& + ty& * 16&)
                c% = (word% AND mask%) \ divisor%
                LINE (sx%, y1&)-(sx% + 1, y2&), c%, BF
            END IF
        NEXT ty&
    ELSE
        FOR ty& = first& TO last&
            y1& = ybase& + (ty& * h&) \ 64&: y2& = ybase& + ((ty& + 1&) * h&) \ 64& - 1&
            IF y1& < top% THEN y1& = top%
            IF y2& > bot% THEN y2& = bot%
            IF y2& >= y1& THEN
                word% = texpack%(texbase& + ty& * 16&)
                c% = (word% AND 28672) \ 4096
                IF word% < 0 THEN c% = c% + 8
                LINE (sx%, y1&)-(sx% + 1, y2&), c%, BF
            END IF
        NEXT ty&
    END IF
END SUB

FUNCTION DimColor% (c%)
    SELECT CASE c%
        CASE 15: DimColor% = 7
        CASE 14: DimColor% = 6
        CASE 13: DimColor% = 5
        CASE 12: DimColor% = 4
        CASE 11: DimColor% = 3
        CASE 10: DimColor% = 2
        CASE 9: DimColor% = 1
        CASE 7: DimColor% = 8
        CASE ELSE: DimColor% = c%
    END SELECT
END FUNCTION

SUB RenderSprites
    DIM order%(1 TO 96), far!(1 TO 96)
    count% = 0
    FOR n% = 1 TO monsters%
        IF spralive%(n%) THEN
            xx! = sprx!(n%) - px!: yy! = spry!(n%) - py!
            forward! = xx! * dx! + yy! * dy!
            lateral! = ABS(xx! * dy! - yy! * dx!)
            IF forward! > .08 AND lateral! < forward! * .9 + .7 THEN
                count% = count% + 1: order%(count%) = n%
                far!(n%) = xx! * xx! + yy! * yy!
            END IF
        END IF
    NEXT n%
    FOR a% = 1 TO count% - 1
        FOR b% = a% + 1 TO count%
            IF far!(order%(a%)) < far!(order%(b%)) THEN SWAP order%(a%), order%(b%)
        NEXT b%
    NEXT a%
    FOR a% = 1 TO count%
        n% = order%(a%)
        RenderSprite n%
    NEXT a%
END SUB

SUB RenderSprite (n%)
    relx! = sprx!(n%) - px!: rely! = spry!(n%) - py!
    det! = planex! * dy! - dx! * planey!
    IF ABS(det!) < .0001 THEN EXIT SUB
    inv! = 1! / det!
    tx! = inv! * (dy! * relx! - dx! * rely!)
    depth! = inv! * (-planey! * relx! + planex! * rely!)
    IF depth! <= .1 THEN EXIT SUB

    center% = INT(160! * (1! + tx! / depth!))
    size% = ABS(INT(96! / depth!))
    IF sprkind%(n%) >= 3 AND sprkind%(n%) <= 10 THEN size% = size% * 2 \ 3
    IF sprkind%(n%) = 2 THEN size% = size% * 3 \ 4
    IF sprkind%(n%) = 12 THEN size% = size% \ 3
    IF size% < 3 THEN EXIT SUB
    IF size% > 150 THEN size% = 150
    sprleft% = center% - size% \ 2
    sprright% = center% + size% \ 2
    IF sprright% < 0 OR sprleft% > 319 THEN EXIT SUB
    top% = 59 - size% \ 2
    bottom% = 59 + size% \ 2

    firstx% = sprleft%: IF firstx% < 0 THEN firstx% = 0
    lastx% = sprright%: IF lastx% > 319 THEN lastx% = 319
    visible% = 0
    FOR testx% = firstx% TO lastx% STEP 8
        IF depth! < zbuffer!(testx% \ 2) THEN visible% = -1
    NEXT testx%
    IF visible% = 0 THEN EXIT SUB

    FOR sx% = sprleft% TO sprright% STEP 2
        IF sx% >= 0 AND sx% < 320 THEN
            ray% = sx% \ 2
            IF depth! < zbuffer!(ray%) THEN
                txp% = ((sx% - sprleft%) * 64) \ (size% + 1)
                IF txp% < 0 THEN txp% = 0
                IF txp% > 63 THEN txp% = 63
                pic% = sprpic%(n%)
                IF sprkind%(n%) = 1 THEN
                    IF sprcool%(n%) > 0 THEN pic% = 3 ELSE pic% = 1 + (frame% \ 8) MOD 2
                END IF
                IF sprkind%(n%) = 2 THEN pic% = 4 + (frame% \ 5) MOD 2
                IF sprkind%(n%) = 12 THEN pic% = 17 + (frame% \ 3) MOD 2
                sprbase& = (pic% - 1) * 1024& + txp% \ 4
                nibble% = txp% AND 3
                IF nibble% = 0 THEN mask% = 15: divisor% = 1
                IF nibble% = 1 THEN mask% = 240: divisor% = 16
                IF nibble% = 2 THEN mask% = 3840: divisor% = 256
                FOR typ% = 0 TO 63
                    word% = sprpack%(sprbase& + typ% * 16&)
                    IF nibble% < 3 THEN
                        c% = (word% AND mask%) \ divisor%
                    ELSE
                        c% = (word% AND 28672) \ 4096
                        IF word% < 0 THEN c% = c% + 8
                    END IF
                    IF c% <> 5 THEN
                        y1% = top% + (typ% * size%) \ 64
                        y2% = top% + ((typ% + 1) * size%) \ 64
                        IF y2% >= 0 AND y1% <= 119 THEN
                            IF y1% < 0 THEN y1% = 0
                            IF y2% > 119 THEN y2% = 119
                            LINE (sx%, y1%)-(sx% + 1, y2%), c%, BF
                        END IF
                    END IF
                NEXT typ%
            END IF
        END IF
    NEXT sx%
END SUB

SUB DrawHUD
    ' Transparent first-person spell hand, composited with a DOS-style mask.
    IF wandphase% > 0 THEN
        PUT (123, handy%), handmask%(0), AND
        PUT (123, handy%), handpic%(0), OR
    END IF
    IF shotflash% > 0 THEN
        LINE (154, 58)-(166, 64), 14, BF
        LINE (157, 53)-(163, 59), 15, BF
    END IF

    ' The real Catacomb Abyss status panel, health faces, radar, and gems are
    ' decoded directly from EGAGRAPH.ABS.
    PUT (0, 120), hud%(0), PSET

    faceoff% = 0
    IF health% <= 75 THEN faceoff% = 482
    IF health% <= 50 THEN faceoff% = 964
    IF health% <= 25 THEN faceoff% = 1446
    IF health% <= 0 THEN faceoff% = 1928
    PUT (64, 134), hudart%(faceoff%), PSET

    PUT (248, 131), hudart%(2410), PSET
    FOR n% = 1 TO monsters%
        IF spralive%(n%) THEN
            vx! = sprx!(n%) - px!: vy! = spry!(n%) - py!
            d2! = vx! * vx! + vy! * vy!
            IF d2! <= 25! THEN
                rx! = vx! * -dy! + vy! * dx!
                ry! = vx! * dx! + vy! * dy!
                dotx% = 276 + INT(rx! * 3!): doty% = 153 - INT(ry! * 3!)
                dotcolor% = 14
                IF sprkind%(n%) = 1 OR sprkind%(n%) = 2 THEN dotcolor% = 12
                IF sprkind%(n%) = 12 THEN dotcolor% = 15
                PSET (dotx%, doty%), dotcolor%
                PSET (dotx% + 1, doty%), dotcolor%
                PSET (dotx%, doty% + 1), dotcolor%
            END IF
        END IF
    NEXT n%
    PSET (276, 153), 15: PSET (275, 153), 15: PSET (277, 153), 15
    PSET (276, 152), 15: PSET (276, 154), 15
    PUT (248, 171), hudart%(2972), PSET
    IF purplegem% THEN PUT (288, 173), hudart%(3334), PSET

    COLOR 14, 8: LOCATE 16, 13: PRINT "TOWNE CEMETARY";
    COLOR 15, 0
    LOCATE 18, 18: PRINT RIGHT$("  " + LTRIM$(STR$(bolts%)), 2);
    LOCATE 20, 18: PRINT " 0";
    LOCATE 23, 18: PRINT RIGHT$("  " + LTRIM$(STR$(potions%)), 2);
    LOCATE 20, 21: PRINT RIGHT$("  " + LTRIM$(STR$(ABS(redkey%))), 2);
    LOCATE 20, 24: PRINT RIGHT$("  " + LTRIM$(STR$(ABS(greenkey%))), 2);
    LOCATE 23, 21: PRINT RIGHT$("  " + LTRIM$(STR$(ABS(bluekey%))), 2);
    LOCATE 23, 24: PRINT RIGHT$("  " + LTRIM$(STR$(ABS(yellowkey%))), 2);
    LOCATE 23, 10: PRINT RIGHT$("   " + LTRIM$(STR$(health%)), 3);
    IF scrolls% > 0 THEN COLOR 13, 0: LOCATE 20, 27: PRINT "S";
    COLOR 15, 0
END SUB

SUB ReadControls
    move$ = "": fire% = 0
    DO
        k$ = INKEY$
        IF k$ = "" THEN EXIT DO
        IF k$ = CHR$(27) THEN quitflag% = -1: EXIT DO
        IF k$ = " " THEN fire% = -1 ELSE move$ = k$
    LOOP
    IF quitflag% THEN EXIT SUB
    k$ = move$
    IF k$ = "" THEN
        IF fire% THEN FireWand
        EXIT SUB
    END IF
    ku$ = UCASE$(k$)

    IF k$ = CHR$(0) + CHR$(72) OR ku$ = "W" THEN TryMove dx! * .18, dy! * .18
    IF k$ = CHR$(0) + CHR$(80) OR ku$ = "S" THEN TryMove -dx! * .15, -dy! * .15
    IF k$ = CHR$(0) + CHR$(75) THEN TurnPlayer -.12
    IF k$ = CHR$(0) + CHR$(77) THEN TurnPlayer .12
    IF ku$ = "A" THEN TryMove dy! * .14, -dx! * .14
    IF ku$ = "D" THEN TryMove -dy! * .14, dx! * .14
    IF fire% THEN FireWand
END SUB

SUB TurnPlayer (amount!)
    cs! = COS(amount!): sn! = SIN(amount!)
    oldx! = dx!
    dx! = dx! * cs! - dy! * sn!
    dy! = oldx! * sn! + dy! * cs!
    oldx! = planex!
    planex! = planex! * cs! - planey! * sn!
    planey! = oldx! * sn! + planey! * cs!
END SUB

SUB TryMove (mx!, my!)
    nx! = px! + mx!: ny! = py! + my!
    tx% = INT(nx! + SGN(mx!) * .18): ty% = INT(py!)
    IF world%(ty% * 40 + tx%) <> 0 THEN opened% = OpenTile%(tx%, ty%)
    IF WallAt%(nx! + SGN(mx!) * .18, py!) = 0 THEN px! = nx!
    tx% = INT(px!): ty% = INT(ny! + SGN(my!) * .18)
    IF world%(ty% * 40 + tx%) <> 0 THEN opened% = OpenTile%(tx%, ty%)
    IF WallAt%(px!, ny! + SGN(my!) * .18) = 0 THEN py! = ny!
END SUB

FUNCTION OpenTile% (tx%, ty%)
    OpenTile% = 0
    IF tx% < 0 OR tx% >= 40 OR ty% < 0 OR ty% >= 28 THEN EXIT FUNCTION
    spot% = ty% * 40 + tx%
    action% = InfoHigh%(info%(spot%))
    IF action% = 253 THEN messageframes% = 40: SOUND 110, 1: EXIT FUNCTION
    IF action% <> 254 AND action% <> 255 THEN EXIT FUNCTION
    needkey% = 0
    IF ty% < 27 THEN needkey% = InfoHigh%(info%(spot% + 40))
    have% = -1
    IF needkey% = 1 THEN have% = redkey%
    IF needkey% = 2 THEN have% = 0
    IF needkey% = 3 THEN have% = greenkey%
    IF needkey% = 4 THEN have% = bluekey%
    IF have% = 0 THEN hurtflash% = 2: SOUND 110, 1: EXIT FUNCTION
    IF needkey% = 1 THEN redkey% = 0
    IF needkey% = 3 THEN greenkey% = 0
    IF needkey% = 4 THEN bluekey% = 0
    IF action% = 255 THEN
        outcome% = 1: quitflag% = -1
    ELSE
        world%(spot%) = 0: info%(spot%) = 0
        score% = score% + 100
        SOUND 660, 1
    END IF
    OpenTile% = -1
END FUNCTION

FUNCTION InfoHigh% (value%)
    IF value% < 0 THEN
        InfoHigh% = value% \ 256 + 256
    ELSE
        InfoHigh% = value% \ 256
    END IF
END FUNCTION

FUNCTION WallAt% (x!, y!)
    IF x! < 0 OR x! >= 40 OR y! < 0 OR y! >= 28 THEN
        WallAt% = 1
    ELSE
        WallAt% = world%(INT(y!) * 40 + INT(x!))
    END IF
END FUNCTION

SUB FireWand
    IF wandphase% = 0 THEN
        wandphase% = 1
        wandstep% = 0
        handy% = 120
    ELSE
        wandphase% = 2
        wandstep% = -1
        handy% = 63
        shotflash% = 3
        ResolveShot
    END IF
END SUB

SUB UpdateWand
    IF wandphase% = 0 THEN EXIT SUB
    IF wandphase% = 1 THEN
        wandstep% = wandstep% + 1
        handy% = 120 - wandstep% * 15
        IF handy% <= 63 THEN
            handy% = 63
            wandphase% = 2
            wandstep% = 0
            shotflash% = 3
            ResolveShot
        END IF
    ELSEIF wandphase% = 2 THEN
        wandstep% = wandstep% + 1
        handy% = 63
        IF wandstep% >= 5 THEN
            wandphase% = 3
            wandstep% = 0
        END IF
    ELSE
        wandstep% = wandstep% + 1
        handy% = 63 + wandstep% * 15
        IF handy% >= 120 THEN
            handy% = 120
            wandphase% = 0
            wandstep% = 0
        END IF
    END IF
END SUB

SUB ResolveShot
    AddObject 12, 17, px! + dx! * .25, py! + dy! * .25, 1
    IF newobject% = 0 THEN EXIT SUB
    sprdx!(newobject%) = dx! * .24: sprdy!(newobject%) = dy! * .24
    SOUND 900, 1
END SUB

SUB UpdateMonsters
    FOR n% = 1 TO monsters%
        IF spralive%(n%) AND sprcool%(n%) > 0 THEN sprcool%(n%) = sprcool%(n%) - 1
        kind% = sprkind%(n%)
        IF spralive%(n%) AND (kind% = 1 OR kind% = 2) THEN
            vx! = px! - sprx!(n%): vy! = py! - spry!(n%)
            d2! = vx! * vx! + vy! * vy!
            IF d2! < 36! AND d2! > .3 AND frame% MOD 2 = 0 THEN
                d! = SQR(d2!)
                speed! = .022
                IF kind% = 2 THEN speed! = .032
                mx! = vx! / d! * speed!: my! = vy! / d! * speed!
                IF WallAt%(sprx!(n%) + mx!, spry!(n%)) = 0 THEN sprx!(n%) = sprx!(n%) + mx!
                IF WallAt%(sprx!(n%), spry!(n%) + my!) = 0 THEN spry!(n%) = spry!(n%) + my!
            END IF
            IF d2! <= .45 AND sprcool%(n%) = 0 THEN
                damage% = 6
                IF kind% = 2 THEN damage% = 3
                health% = health% - damage%: hurtflash% = 3: sprcool%(n%) = 30
                SOUND 90, 1
                IF health% <= 0 THEN health% = 0: outcome% = 2: quitflag% = -1
            END IF
        END IF
        IF spralive%(n%) AND kind% >= 3 AND kind% <= 10 THEN
            vx! = px! - sprx!(n%): vy! = py! - spry!(n%)
            IF vx! * vx! + vy! * vy! < .32 THEN CollectObject n%
        END IF
        IF spralive%(n%) AND kind% = 12 THEN
            oldx! = sprx!(n%): oldy! = spry!(n%)
            sprx!(n%) = sprx!(n%) + sprdx!(n%): spry!(n%) = spry!(n%) + sprdy!(n%)
            wx% = INT(sprx!(n%)): wy% = INT(spry!(n%))
            IF WallAt%(sprx!(n%), spry!(n%)) <> 0 THEN
                spot% = wy% * 40 + wx%
                IF spot% >= 0 AND spot% <= 1119 THEN
                    IF InfoHigh%(info%(spot%)) = 252 THEN
                        world%(spot%) = 0: info%(spot%) = 0: score% = score% + 25
                        SOUND 120, 1
                    END IF
                END IF
                spralive%(n%) = 0
            ELSE
                FOR e% = 1 TO monsters%
                    IF spralive%(n%) AND spralive%(e%) AND (sprkind%(e%) = 1 OR sprkind%(e%) = 2) THEN
                        hx! = sprx!(e%) - sprx!(n%): hy! = spry!(e%) - spry!(n%)
                        IF hx! * hx! + hy! * hy! < .18 THEN DamageEnemy e%, 1: spralive%(n%) = 0
                    END IF
                NEXT e%
            END IF
            IF spralive%(n%) THEN
                vx! = sprx!(n%) - px!: vy! = spry!(n%) - py!
                IF vx! * vx! + vy! * vy! > 64! THEN spralive%(n%) = 0
            END IF
        END IF
    NEXT n%
END SUB

SUB DamageEnemy (n%, damage%)
    sprhp%(n%) = sprhp%(n%) - damage%
    IF sprhp%(n%) <= 0 THEN
        spralive%(n%) = 0
        IF sprkind%(n%) = 1 THEN score% = score% + 100
        IF sprkind%(n%) = 2 THEN score% = score% + 50
        SOUND 180, 1
    ELSE
        sprcool%(n%) = 5
        SOUND 300, 1
    END IF
END SUB

SUB CollectObject (n%)
    kind% = sprkind%(n%)
    spralive%(n%) = 0
    SELECT CASE kind%
        CASE 3
            bolts% = bolts% + 1: score% = score% + 25
        CASE 4
            potions% = potions% + 1
            health% = health% + 25: IF health% > 100 THEN health% = 100
        CASE 5
            redkey% = -1: score% = score% + 50
        CASE 6
            greenkey% = -1: score% = score% + 50
        CASE 7
            bluekey% = -1: score% = score% + 50
        CASE 8
            purplegem% = -1: score% = score% + 500: treasures% = treasures% + 1
        CASE 9
            scrolls% = scrolls% + 1: score% = score% + 1000: treasures% = treasures% + 1
        CASE 10
            score% = score% + 250: treasures% = treasures% + 1
    END SELECT
    SOUND 720, 1
END SUB
