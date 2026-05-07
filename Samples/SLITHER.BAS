' Slither.io QB71 - Multi-Bot Eating Survivors
' Translated from Slither.io QBJS by Elisson Fabiano + AI
WIDTH 80, 50
SCREEN 0, , 1, 0
RANDOMIZE TIMER
' --- Configuracoes ---
CONST WORLDW = 150
CONST WORLDH = 80
CONST SCRW = 80
CONST SCRH = 50
CONST NBOTS = 5 ' Total de 6 bots (0 a 5)
CONST NFOOD = 60
DIM SHARED pX(500), pY(500), pLen, pDX, pDY
DIM SHARED bX(5, 500), bY(5, 500), bLen(5), bDX(5), bDY(5)
DIM SHARED fX(100), fY(100), fT(100)
DIM SHARED score, gameover, camX, camY
DIM SHARED turboCount, movs
kUP$ = CHR$(0) + "H"
kDOWN$ = CHR$(0) + "P"
kLEFT$ = CHR$(0) + "K"
kRIGHT$ = CHR$(0) + "M"
' --- Inicializacao ---
pLen = 15
pX(0) = 75
pY(0) = 40
pDX = 1
pDY = 0
gameover = 0
turboCount = 0
FOR i = 0 TO NBOTS
    bLen(i) = 15
    bX(i, 0) = INT(RND * WORLDW)
    bY(i, 0) = INT(RND * WORLDH)
    bDX(i) = 1
    bDY(i) = 0
NEXT i
FOR i = 0 TO NFOOD
    fX(i) = INT(RND * WORLDW)
    fY(i) = INT(RND * WORLDH)
    fT(i) = 1
NEXT i
' --- Loop Principal ---
DO WHILE gameover = 0
    st# = TIMER: WHILE st# = TIMER: WEND
    st# = TIMER: WHILE st# = TIMER: WEND

    CLS
    ' 1. Teclado
    k$ = INKEY$
    IF k$ = kUP$ AND pDY <> 1 THEN
        pDX = 0
        pDY = -1
    END IF
    IF k$ = kDOWN$ AND pDY <> -1 THEN
        pDX = 0
        pDY = 1
    END IF
    IF k$ = kLEFT$ AND pDX <> 1 THEN
        pDX = -1
        pDY = 0
    END IF
    IF k$ = kRIGHT$ AND pDX <> -1 THEN
        pDX = 1
        pDY = 0
    END IF
    ' 2. Turbo e Movimento Player
    movs = 1
    IF (INP(&H60) AND 1) = 1 AND pLen > 5 THEN
        movs = 2
    END IF
    FOR m = 1 TO movs
        FOR i = pLen TO 1 STEP -1
            pX(i) = pX(i - 1)
            pY(i) = pY(i - 1)
        NEXT i
        pX(0) = (pX(0) + pDX + WORLDW) MOD WORLDW
        pY(0) = (pY(0) + pDY + WORLDH) MOD WORLDH
        IF m = 2 THEN
            turboCount = turboCount + 1
            IF turboCount MOD 3 = 0 THEN
                pLen = pLen - 1
                idxT = INT(RND * NFOOD)
                fX(idxT) = pX(pLen)
                fY(idxT) = pY(pLen)
                fT(idxT) = 2
            END IF
        END IF
    NEXT m
    camX = pX(0) - (SCRW / 2)
    camY = pY(0) - (SCRH / 2)
    ' 3. IA e Movimento dos BOTS
    FOR b = 0 TO NBOTS
        ' IA DE FOME: Achar comida mais proxima
        minD = 1000
        tIdx = 0
        FOR i = 0 TO NFOOD
            dist = ABS(bX(b, 0) - fX(i)) + ABS(bY(b, 0) - fY(i))
            IF dist < minD THEN
                minD = dist
                tIdx = i
            END IF
        NEXT i
        ' Direcionar (Trava 180)
        IF RND < .3 THEN
            oldX = bDX(b)
            oldY = bDY(b)
            IF fX(tIdx) > bX(b, 0) AND oldX <> -1 THEN
                bDX(b) = 1
                bDY(b) = 0
            ELSEIF fX(tIdx) < bX(b, 0) AND oldX <> 1 THEN
                bDX(b) = -1
                bDY(b) = 0
            ELSEIF fY(tIdx) > bY(b, 0) AND oldY <> -1 THEN
                bDX(b) = 0
                bDY(b) = 1
            ELSEIF fY(tIdx) < bY(b, 0) AND oldY <> 1 THEN
                bDX(b) = 0
                bDY(b) = -1
            END IF
        END IF
        ' SENSOR DE SOBREVIVENCIA (Evita Player e Outros Bots)
        nx = (bX(b, 0) + bDX(b) + WORLDW) MOD WORLDW
        ny = (bY(b, 0) + bDY(b) + WORLDH) MOD WORLDH
        danger = 0
        ' Contra Player
        FOR s = 0 TO pLen
            IF nx = pX(s) AND ny = pY(s) THEN
                danger = 1
            END IF
        NEXT s
        ' Contra outros Bots
        FOR ob = 0 TO NBOTS
            IF ob <> b THEN
                FOR sb = 0 TO bLen(ob)
                    IF nx = bX(ob, sb) AND ny = bY(ob, sb) THEN
                        danger = 1
                    END IF
                NEXT sb
            END IF
        NEXT ob
        IF danger = 1 THEN
            tDX = bDX(b)
            bDX(b) = bDY(b)
            bDY(b) = -tDX
        END IF
        ' Mover Bot
        FOR i = bLen(b) TO 1 STEP -1
            bX(b, i) = bX(b, i - 1)
            bY(b, i) = bY(b, i - 1)
        NEXT i
        bX(b, 0) = (bX(b, 0) + bDX(b) + WORLDW) MOD WORLDW
        bY(b, 0) = (bY(b, 0) + bDY(b) + WORLDH) MOD WORLDH
        ' Bot Come
        FOR f = 0 TO NFOOD
            IF bX(b, 0) = fX(f) AND bY(b, 0) = fY(f) THEN
                IF fT(f) = 1 THEN
                    foodEaten = 1
                ELSE
                    foodEaten = 3
                END IF

                FOR i = bLen(b) + 1 TO bLen(b) + foodEaten
                    bX(b, i) = bX(b, i - 1)
                    bY(b, i) = bY(b, i - 1)
                NEXT i

                bLen(b) = bLen(b) + foodEaten

                fX(f) = INT(RND * WORLDW)
                fY(f) = INT(RND * WORLDH)
                fT(f) = 1
            END IF
        NEXT f
    NEXT b
    ' 4. Desenhar Comida
    FOR i = 0 TO NFOOD
        FOR ox = -1 TO 1
            FOR oy = -1 TO 1
                dX = (fX(i) + ox * WORLDW) - camX
                dY = (fY(i) + oy * WORLDH) - camY
                IF dX > 0 AND dX <= SCRW AND dY > 1 AND dY <= SCRH THEN
                    IF fT(i) = 1 THEN
                        COLOR 14
                        LOCATE dY, dX
                        PRINT CHR$(5);
                    ELSE
                        COLOR 11
                        LOCATE dY, dX
                        PRINT CHR$(4);
                    END IF
                END IF
            NEXT oy
        NEXT ox
        IF pX(0) = fX(i) AND pY(0) = fY(i) THEN
            IF fT(i) = 1 THEN
                foodEaten = 1
            ELSE
                foodEaten = 3
            END IF

            FOR i = pLen + 1 TO pLen + foodEaten
                pX(i) = pX(i - 1)
                pY(i) = pY(i - 1)
            NEXT i

            pLen = pLen + foodEaten
            score = score + foodEaten * 10

            fX(i) = INT(RND * WORLDW)
            fY(i) = INT(RND * WORLDH)
            fT(i) = 1
        END IF
    NEXT i
    ' 5. Colisoes Fatais e Desenho de Bots
    FOR b = 0 TO NBOTS
        ' Bot Morre se bater no Player ou outros
        botM = 0
        FOR s = 0 TO pLen
            IF bX(b, 0) = pX(s) AND bY(b, 0) = pY(s) THEN
                botM = 1
            END IF
        NEXT s
        ' Contra outros bots
        FOR ob2 = 0 TO NBOTS
            IF ob2 <> b THEN
                FOR sb2 = 0 TO bLen(ob2)
                    IF bX(b, 0) = bX(ob2, sb2) AND bY(b, 0) = bY(ob2, sb2) THEN
                        botM = 1
                    END IF
                NEXT sb2
            END IF
        NEXT ob2
        IF botM = 1 THEN
            FOR mbody = 0 TO bLen(b) STEP 2
                fidx = INT(RND * NFOOD)
                fX(fidx) = bX(b, mbody)
                fY(fidx) = bY(b, mbody)
                fT(fidx) = 2
            NEXT mbody
            bLen(b) = 15
            bX(b, 0) = INT(RND * WORLDW)
            bY(b, 0) = INT(RND * WORLDH)
            FOR i = 1 TO bLen(b)
                bX(b, i) = bX(b, i - 1)
                bY(b, i) = bY(b, i - 1)
            NEXT i
        END IF
        ' Player morre se bater no bot
        FOR i = 0 TO bLen(b)
            IF pX(0) = bX(b, i) AND pY(0) = bY(b, i) THEN
                gameover = 1
            END IF
        NEXT i
        ' Desenhar Bots
        COLOR 12
        FOR i = bLen(b) TO 0 STEP -1
            FOR ox = -1 TO 1
                FOR oy = -1 TO 1
                    dX = (bX(b, i) + ox * WORLDW) - camX
                    dY = (bY(b, i) + oy * WORLDH) - camY
                    IF dX > 0 AND dX <= SCRW AND dY > 1 AND dY <= SCRH THEN
                        LOCATE dY, dX
                        IF i = 0 THEN
                            PRINT CHR$(1);
                        ELSE
                            PRINT CHR$(219);
                        END IF
                    END IF
                NEXT oy
            NEXT ox
        NEXT i
    NEXT b
    ' 6. Desenhar Jogador
    pC = 10
    IF pLen > 30 THEN pC = 11
    IF pLen > 60 THEN pC = 15
    COLOR pC
    FOR i = pLen TO 0 STEP -1
        FOR ox = -1 TO 1
            FOR oy = -1 TO 1
                dX = (pX(i) + ox * WORLDW) - camX
                dY = (pY(i) + oy * WORLDH) - camY
                IF dX > 0 AND dX <= SCRW AND dY > 1 AND dY <= SCRH THEN
                    LOCATE dY, dX
                    IF i = 0 THEN
                        PRINT CHR$(2);
                    ELSE
                        PRINT CHR$(176);
                    END IF
                END IF
            NEXT oy
        NEXT ox
    NEXT i
    COLOR 15
    LOCATE 1, 1
    PRINT "PTS:"; score;
    PCOPY 1, 0
LOOP
SCREEN 0, , 0, 0
CLS : COLOR 12: LOCATE 12, 33: PRINT "GAME OVER!": END

