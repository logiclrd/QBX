DECLARE SUB INITIALIZE ()
DECLARE SUB MENUBAR ()
DECLARE SUB COLORBAR ()
'$DYNAMIC
DIM SHARED THEBAR%(7, 319)
TYPE REGTYPE
    AX AS INTEGER
    BX AS INTEGER
    CX AS INTEGER
    DX AS INTEGER
    BP AS INTEGER
    SI AS INTEGER
    DI AS INTEGER
    FLAGS AS INTEGER
    DS AS INTEGER
    ES AS INTEGER
END TYPE
    DIM SHARED INREGS AS REGTYPE, OUTREGS AS REGTYPE, SAVED%
    DIM STORAGE(30721) AS INTEGER
    DIM SHARED FORE%, BACK%, STARTUP%, ONLY1%, ONLY2%, DRAWFLAG%, BRUSHSIZE%, SPEED%
    DIM SHARED SPRAYFLAG%, SHADEFLAG%, LINEFLAG%, BOXFLAG%, CIRCLEFLAG%, CLRONLY%
    DIM SHARED PAINTFLAG%
    INITIALIZE
20 DO
        GOSUB GETLEFTBUTTONPRESS
        ROW% = OUTREGS.DX \ 8 + 1
        IF ROW% = 1 THEN
            MENUSELECTIONMADE% = -1
        ELSE
            MENUSELECTIONMADE% = 0
        END IF
        IF OUTREGS.CX < 73 AND OUTREGS.DX > 19 AND OUTREGS.DX < 160 THEN
            COLORSELECTIONMADE% = -1
        ELSE
            COLORSELECTIONMADE% = 0
        END IF
        IF (OUTREGS.CX \ 2 > 2) AND (OUTREGS.CX \ 2 < 29) AND (OUTREGS.DX > 167) AND (OUTREGS.DX < 178) THEN
            IF SPRAYFLAG% = -1 THEN SPRAYFLAG% = 0 ELSE SPRAYFLAG% = -1
            SPRAYCHANGED% = -1
            COLORBAR
            DO
                INREGS.AX = 6
                INREGS.BX = 0
                CALL INTERRUPT(&H33, INREGS, OUTREGS)
            LOOP WHILE OUTREGS.BX = 0
        ELSE
            SPRAYCHANGED% = 0
            DO
                INREGS.AX = 6
                INREGS.BX = 0
                CALL INTERRUPT(&H33, INREGS, OUTREGS)
            LOOP WHILE OUTREGS.BX = 0
        END IF
        IF (OUTREGS.CX \ 2 > 2) AND (OUTREGS.CX \ 2 < 29) AND (OUTREGS.DX > 179) AND (OUTREGS.DX < 190) THEN
            IF SHADEFLAG% = -1 THEN SHADEFLAG% = 0 ELSE SHADEFLAG% = -1
            SHADECHANGED% = -1
            COLORBAR
            DO
                INREGS.AX = 3
                CALL INTERRUPT(&H33, INREGS, OUTREGS)
            LOOP UNTIL OUTREGS.BX = 0
        ELSE
            SHADECHANGED% = 0
            DO
                INREGS.AX = 3
                CALL INTERRUPT(&H33, INREGS, OUTREGS)
            LOOP UNTIL OUTREGS.BX = 0
        END IF
        IF MENUSELECTIONMADE% THEN
            GOSUB MENUCHOICE
        ELSEIF COLORSELECTIONMADE% THEN
            GOSUB COLORCHOICE
        ELSEIF (NOT SPRAYCHANGED%) AND (NOT SHADECHANGED%) THEN
            IF DRAWFLAG% THEN GOSUB DRAWPIXEL
            IF LINEFLAG% THEN GOSUB DRAWLINE
            IF BOXFLAG% THEN GOSUB DRAWBOX
            IF CIRCLEFLAG% THEN GOSUB DRAWCIRCLE
            IF PAINTFLAG% THEN GOSUB DRAWPAINT
        END IF
    LOOP
    SYSTEM
GETLEFTBUTTONPRESS:
    DO
        INREGS.BX = 0
        INREGS.AX = 3
        CALL INTERRUPT(&H33, INREGS, OUTREGS)
    LOOP WHILE OUTREGS.BX = 0
    IF OUTREGS.BX = 2 THEN
        COLOR 15
        INREGS.AX = 2
        CALL INTERRUPT(&H33, INREGS, OUTREGS)
        GOSUB PREPARE
        CLS
        CIRCLE (160, 100), INT(BRUSHSIZE%) * 1.25, 15
        TEXT$ = "Use Left and Right keys for density"
        ROW = 23
        GOSUB CENTER
        TEXT$ = "Use Up and Down keys to size brush"
        ROW = 24
        GOSUB CENTER
        TEXT$ = "Press Enter when done"
        ROW = 25
        GOSUB CENTER
        VIEW PRINT 1 TO 22
        DO
            DO
                a$ = INKEY$
                FOR I% = 1 TO SPEED%
                    a# = RND * 6.28
                    D% = RND * BRUSHSIZE%
                    PSET (160 + (SIN(a#) * D%), 100 + (COS(a#) * D%)), 7
                NEXT I%
                COUNT% = COUNT% + 1
                IF COUNT% = BRUSHSIZE% * 100 THEN CLS 2: CIRCLE (160, 100), INT(BRUSHSIZE%) * 1.25: COUNT% = 0
                INREGS.AX% = 3
                CALL INTERRUPT(&H33, INREGS, OUTREGS)
                INREGS.AX% = 6
                INREGS.BX% = 0
                CALL INTERRUPT(&H33, INREGS, OUTREGS)
            LOOP UNTIL a$ <> ""
            SELECT CASE a$
                CASE CHR$(0) + "H": BRUSHSIZE% = BRUSHSIZE% + 1
                CASE CHR$(0) + "P": BRUSHSIZE% = BRUSHSIZE% - 1
                CASE CHR$(0) + "K": SPEED% = SPEED% - 1
                CASE CHR$(0) + "M": SPEED% = SPEED% + 1
            END SELECT
            IF BRUSHSIZE% = 4 THEN BRUSHSIZE% = 5
            IF BRUSHSIZE% = 66 THEN BRUSHSIZE% = 65
            IF SPEED% = 0 THEN SPEED% = 1
            IF SPEED% = 21 THEN SPEED% = 20
            IF OLDSIZE% <> BRUSHSIZE% THEN
                CLS 2
                CIRCLE (160, 100), INT(BRUSHSIZE%) * 1.25, 15
            END IF
            OLDSIZE% = BRUSHSIZE%
        LOOP UNTIL a$ = CHR$(13)
        CLS
        MENUBAR
        PUT (0, 8), STORAGE
        COLORBAR
        VIEW PRINT
        INREGS.AX = 1
        CALL INTERRUPT(&H33, INREGS, OUTREGS)
        GOTO GETLEFTBUTTONPRESS
    END IF
RETURN
MENUCHOICE:
    CHOICE% = OUTREGS.CX \ 64 + 1
    SELECT CASE CHOICE%
        CASE 1
            COLOR 15
            INREGS.AX = 2
            CALL INTERRUPT(&H33, INREGS, OUTREGS)
            IF NOT SAVED THEN
            TEXT$ = "ษออออออออออออออออออออป"
            ROW = 11
            GOSUB CENTER
            TEXT$ = "บ Save file?         บ"
            ROW = 12
            GOSUB CENTER
            TEXT$ = "ศออออออออออออออออออออผ"
            ROW = 13
            GOSUB CENTER
            DO
            KEY$ = UCASE$(INKEY$)
            LOOP UNTIL KEY$ = "Y" OR KEY$ = "N"
            IF KEY$ = "N" THEN
            TEXT$ = "ษออออออออออออออออออออป"
            ROW = 11
            GOSUB CENTER
            TEXT$ = "บ Save file? N       บ"
            ROW = 12
            GOSUB CENTER
            TEXT$ = "ศออออออออออออออออออออผ"
            ROW = 13
            GOSUB CENTER
            ELSE
            TEXT$ = "ษออออออออออออออออออออป"
            ROW = 11
            GOSUB CENTER
            TEXT$ = "บ Save file? Y       บ"
            ROW = 12
            GOSUB CENTER
            TEXT$ = "ศออออออออออออออออออออผ"
            ROW = 13
            GOSUB CENTER
            GOSUB SAVEFILE
            END IF
            END IF
            SCREEN 0: WIDTH 80, 25
            COLOR 11, 0
            PRINT "Thanks for using the Paint2 drawing program written in QB45!!!!"
            PRINT "Please send comments or suggestions to:"
            PRINT
            PRINT "                         ArCoSoft Unincorporated"
            PRINT "                         Jonathan Gilbert"
            PRINT "                         44 King's Drive"
            PRINT "                         Winnipeg,MB"
            PRINT "                         R3T 3E5"
            PRINT "                         Canada"
            PRINT
            PRINT "Goodbye..."
            PRINT
            COLOR 7, 0
            SYSTEM
        CASE 2
            FORE% = FORE% + 1
            IF FORE% = BACK% THEN FORE% = FORE% + 1
            IF FORE% > 255 THEN FORE% = 0
            COLOR FORE%
            INREGS.AX = 2
            CALL INTERRUPT(&H33, INREGS, OUTREGS)
            CLRONLY% = 1
            MENUBAR
            CLRONLY% = 0
            COLOR FORE%
            INREGS.AX = 1
            CALL INTERRUPT(&H33, INREGS, OUTREGS)
        CASE 3
            BACK% = BACK% + 1
            IF BACK% > 255 THEN BACK% = 0
            COLOR BACK
            INREGS.AX = 2
            CALL INTERRUPT(&H33, INREGS, OUTREGS)
            CLRONLY% = 1
            MENUBAR
            CLRONLY% = 0
            COLOR FORE%
            INREGS.AX = 1
            CALL INTERRUPT(&H33, INREGS, OUTREGS)
        CASE 4
            IF DRAWFLAG% = -1 THEN ONLY2% = 1
            IF LINEFLAG% = -1 THEN ONLY2% = 2
            IF BOXFLAG% = -1 THEN ONLY2% = 3
            IF CIRCLEFLAG% = -1 THEN ONLY2% = 4
            IF PAINTFLAG% = -1 THEN ONLY2% = 5
            DRAWFLAG% = -1: ONLY1% = 1
            LINEFLAG% = 0
            BOXFLAG% = 0
            CIRCLEFLAG% = 0
            PAINTFLAG% = 0
            MENUBAR
            COLORBAR
            ONLY1% = -1: ONLY2% = -1
        CASE 5
            IF DRAWFLAG% = -1 THEN ONLY2% = 1
            IF LINEFLAG% = -1 THEN ONLY2% = 2
            IF BOXFLAG% = -1 THEN ONLY2% = 3
            IF CIRCLEFLAG% = -1 THEN ONLY2% = 4
            IF PAINTFLAG% = -1 THEN ONLY2% = 5
            DRAWFLAG% = 0
            LINEFLAG% = -1: ONLY1% = 2
            BOXFLAG% = 0
            CIRCLEFLAG% = 0
            PAINTFLAG% = 0
            MENUBAR
            COLORBAR
            ONLY1% = -1: ONLY2% = -1
        CASE 6
            IF DRAWFLAG% = -1 THEN ONLY2% = 1
            IF LINEFLAG% = -1 THEN ONLY2% = 2
            IF BOXFLAG% = -1 THEN ONLY2% = 3
            IF CIRCLEFLAG% = -1 THEN ONLY2% = 4
            IF PAINTFLAG% = -1 THEN ONLY2% = 5
            DRAWFLAG% = 0
            LINEFLAG% = 0
            BOXFLAG% = -1: ONLY1% = 3
            CIRCLEFLAG% = 0
            PAINTFLAG% = 0
            MENUBAR
            COLORBAR
            ONLY1% = -1: ONLY2% = -1
        CASE 7
            IF DRAWFLAG% = -1 THEN ONLY2% = 1
            IF LINEFLAG% = -1 THEN ONLY2% = 2
            IF BOXFLAG% = -1 THEN ONLY2% = 3
            IF CIRCLEFLAG% = -1 THEN ONLY2% = 4
            IF PAINTFLAG% = -1 THEN ONLY2% = 5
            DRAWFLAG% = 0
            LINEFLAG% = 0
            BOXFLAG% = 0
            CIRCLEFLAG% = -1: ONLY1% = 4
            PAINTFLAG% = 0
            MENUBAR
            COLORBAR
            ONLY1% = -1: ONLY2% = -1
        CASE 8
            IF DRAWFLAG% = -1 THEN ONLY2% = 1
            IF LINEFLAG% = -1 THEN ONLY2% = 2
            IF BOXFLAG% = -1 THEN ONLY2% = 3
            IF CIRCLEFLAG% = -1 THEN ONLY2% = 4
            IF PAINTFLAG% = -1 THEN ONLY2% = 5
            DRAWFLAG% = 0
            LINEFLAG% = 0
            BOXFLAG% = 0
            CIRCLEFLAG% = 0
            PAINTFLAG% = -1: ONLY1% = 5
            MENUBAR
            COLORBAR
            ONLY1% = -1: ONLY2% = -1
        CASE 9
            GOSUB SAVEFILE
        CASE 10
            GOSUB LOADFILE
        END SELECT
    RETURN
COLORCHOICE:
    IF OUTREGS.CX < 32 THEN INTENSE = 0 ELSE INTENSE = -1
    Y% = OUTREGS.DX
    IF (Y% > 19) AND (Y% < 31) THEN
        BASECOLOR% = (BASECOLOR% + 16) MOD 256
        COLORBAR
    ELSE
        Y% = Y% - 32
        IF Y% >= 0 AND Y% <= 15 THEN Y% = 0
        IF Y% >= 16 AND Y% <= 31 THEN Y% = 1
        IF Y% >= 32 AND Y% <= 47 THEN Y% = 2
        IF Y% >= 48 AND Y% <= 63 THEN Y% = 3
        IF Y% >= 64 AND Y% <= 79 THEN Y% = 4
        IF Y% >= 80 AND Y% <= 95 THEN Y% = 5
        IF Y% >= 96 AND Y% <= 111 THEN Y% = 6
        IF Y% >= 112 AND Y% <= 128 THEN Y% = 7
        cl% = Y%
        IF INTENSE THEN cl% = cl% + 8
        FORE% = cl% + BASECOLOR%
        MENUBAR
    END IF
    RETURN
DRAWPIXEL:
    SAVED = 0
    INREGS.BX = 0
    INREGS.AX = 6
    CALL INTERRUPT(&H33, INREGS, OUTREGS)
    INREGS.AX = 3
    CALL INTERRUPT(&H33, INREGS, OUTREGS)
    X% = OUTREGS.CX \ 2
    Y% = OUTREGS.DX
    COLOR FORE%
    PSET (X%, Y%)
    DO
        INREGS.AX = 3
        CALL INTERRUPT(&H33, INREGS, OUTREGS)
        X% = OUTREGS.CX \ 2
        Y% = OUTREGS.DX
        WAIT &H3DA, 1
        INREGS.AX = 2
        CALL INTERRUPT(&H33, INREGS, OUTREGS)
        IF SPRAYFLAG% = 0 THEN
            LINE -(X%, Y%)
        ELSE
            FOR I% = 1 TO SPEED%
                a# = RND * 6.28
                D% = RND * BRUSHSIZE%
                IF SHADEFLAG% THEN
                    CUR% = POINT(X% + (SIN(a#) * D%), Y% + (COS(a#) * D%))
                    IF CUR% > 31 THEN CUR% = 0
                    CLR% = (15 - (INT(D%) / INT(BRUSHSIZE%)) * 15) + 16
                    IF CLR% < CUR% THEN CLR% = CUR%
                ELSE
                    CLR% = FORE%
                END IF
                PSET (X% + (SIN(a#) * D%), Y% + (COS(a#) * D%)), CLR%
            NEXT I%
        END IF
        INREGS.AX = 1
        CALL INTERRUPT(&H33, INREGS, OUTREGS)
        INREGS.BX = 0
        INREGS.AX = 6
        CALL INTERRUPT(&H33, INREGS, OUTREGS)
    LOOP WHILE OUTREGS.BX = 0
    COLORBAR
    RETURN
DRAWLINE:
    SAVED = 0
    GOSUB PREPARE
    INREGS.AX = 3
    CALL INTERRUPT(&H33, INREGS, OUTREGS)
    X% = OUTREGS.CX \ 2
    Y% = OUTREGS.DX
    XOLD% = X%
    YOLD% = Y%
    INREGS.BX = 0
    INREGS.AX = 6
    CALL INTERRUPT(&H33, INREGS, OUTREGS)
    DO
        INREGS.AX = 3
        CALL INTERRUPT(&H33, INREGS, OUTREGS)
        YNEW% = OUTREGS.DX
        XNEW% = OUTREGS.CX \ 2
        WAIT &H3DA, 1
        INREGS.AX = 2
        CALL INTERRUPT(&H33, INREGS, OUTREGS)
        IF XOLD% <> XNEW% OR YOLD% <> YNEW% THEN
            PUT (0, 8), STORAGE, PSET
            LINE (X%, Y%)-(XNEW%, YNEW%), FORE%
        END IF
        XOLD% = XNEW%
        YOLD% = YNEW%
        INREGS.AX = 1
        CALL INTERRUPT(&H33, INREGS, OUTREGS)
        INREGS.BX = 0
        INREGS.AX = 6
        CALL INTERRUPT(&H33, INREGS, OUTREGS)
    LOOP WHILE OUTREGS.BX = 0
    COLORBAR
    RETURN
DRAWBOX:
    SAVED = 0
    GOSUB PREPARE
    INREGS.AX = 3
    CALL INTERRUPT(&H33, INREGS, OUTREGS)
    X% = OUTREGS.CX \ 2
    Y% = OUTREGS.DX
    XOLD% = X%
    YOLD% = Y%
    INREGS.BX = 0
    INREGS.AX = 6
    CALL INTERRUPT(&H33, INREGS, OUTREGS)
    DO
        INREGS.AX = 3
        CALL INTERRUPT(&H33, INREGS, OUTREGS)
        XNEW% = OUTREGS.CX \ 2
        YNEW% = OUTREGS.DX
        WAIT &H3DA, 1
        INREGS.AX = 2
        CALL INTERRUPT(&H33, INREGS, OUTREGS)
        IF XOLD% <> XNEW% OR YOLD% <> YNEW% THEN
            PUT (0, 8), STORAGE, PSET
            LINE (X%, Y%)-(XNEW%, YNEW%), FORE%, B
        END IF
        XOLD% = XNEW%
        YOLD% = YNEW%
        INREGS.AX = 1
        CALL INTERRUPT(&H33, INREGS, OUTREGS)
        INREGS.BX = 0
        INREGS.AX = 6
        CALL INTERRUPT(&H33, INREGS, OUTREGS)
    LOOP WHILE OUTREGS.BX = 0
    COLORBAR
    RETURN
DRAWCIRCLE:
    SAVED = 0
    GOSUB PREPARE
    INREGS.AX = 3
    CALL INTERRUPT(&H33, INREGS, OUTREGS)
    X% = OUTREGS.CX \ 2
    Y% = OUTREGS.DX
    XOLD% = X%
    YOLD% = Y%
    INREGS.BX = 0
    INREGS.AX = 6
    CALL INTERRUPT(&H33, INREGS, OUTREGS)
    DO
        INREGS.AX = 3
        CALL INTERRUPT(&H33, INREGS, OUTREGS)
        XNEW% = OUTREGS.CX \ 2
        YNEW% = OUTREGS.DX
        WAIT &H3DA, 1
        INREGS.AX = 2
        CALL INTERRUPT(&H33, INREGS, OUTREGS)
        IF XOLD% <> XNEW% OR YOLD% <> YNEW% THEN
            PUT (0, 8), STORAGE, PSET
            IF Y% <> YNEW% AND X% <> XNEW% THEN
                CIRCLE (X%, Y%), ABS(XNEW% - X%) \ 2, FORE%, , , ((YNEW% - Y%) / (XNEW% - X%))
            END IF
        END IF
        XOLD% = XNEW%
        YOLD% = YNEW%
        INREGS.AX = 1
        CALL INTERRUPT(&H33, INREGS, OUTREGS)
        INREGS.BX = 0
        INREGS.AX = 6
        CALL INTERRUPT(&H33, INREGS, OUTREGS)
    LOOP WHILE OUTREGS.BX = 0
    COLORBAR
    RETURN
DRAWPAINT:
    SAVED = 0
    X% = OUTREGS.CX \ 2
    Y% = OUTREGS.DX
    INREGS.AX = 2
    CALL INTERRUPT(&H33, INREGS, OUTREGS)
    PAINT (X%, Y%), FORE%, BACK%
    INREGS.AX = 1
    CALL INTERRUPT(&H33, INREGS, OUTREGS)
    MENUBAR
    COLORBAR
    RETURN
PREPARE:
    INREGS.AX = 2
    CALL INTERRUPT(&H33, INREGS, OUTREGS)
    GET (0, 8)-(319, 199), STORAGE
    INREGS.AX = 1
    CALL INTERRUPT(&H33, INREGS, OUTREGS)
    RETURN
CENTER:
    LOCATE ROW, 20 - LEN(TEXT$) / 2
    PRINT TEXT$;
    RETURN
SAVEFILE:   
    COLOR 15
    SAVED = -1
    GOSUB PREPARE
    INREGS.AX = 2
    CALL INTERRUPT(&H33, INREGS, OUTREGS)
    TEXT$ = "ษออออออออออออออออออออป"
    ROW = 11
    GOSUB CENTER
    TEXT$ = "บ Filename (no ext)  บ"
    ROW = 12
    GOSUB CENTER
    TEXT$ = "บ :                  บ"
    ROW = 13
    GOSUB CENTER
    TEXT$ = "ศออออออออออออออออออออผ"
    ROW = 14
    GOSUB CENTER
    LOCATE 13, 13
    LINE INPUT FILNAM$
    PUT (0, 8), STORAGE, PSET
    FILNAM$ = FILNAM$ + ".BHP"
    LOCATE 1, 1
    PRINT SPACE$(39); : LOCATE 1, 1: PRINT "Click on upper left corner..."
    INREGS.AX = 1
    CALL INTERRUPT(&H33, INREGS, OUTREGS)
    GOSUB GETLEFTBUTTONPRESS
    X1% = OUTREGS.CX \ 2
    Y1% = OUTREGS.DX
    INREGS.AX = 2
    CALL INTERRUPT(&H33, INREGS, OUTREGS)
    DO
        INREGS.AX = 3
        CALL INTERRUPT(&H33, INREGS, OUTREGS)
    LOOP UNTIL OUTREGS.BX = 0
    PSET (X1% - 1, Y1% - 1), 15
    LOCATE 1, 1
    PRINT SPACE$(39); : LOCATE 1, 1: PRINT "Click on lower right corner...";
    INREGS.AX = 1
    CALL INTERRUPT(&H33, INREGS, OUTREGS)
    GOSUB GETLEFTBUTTONPRESS
    X2% = OUTREGS.CX \ 2
    Y2% = OUTREGS.DX
    IF X2% < X1% THEN SWAP X1%, X2%
    IF Y2% < Y1% THEN SWAP Y1%, Y2%
    INREGS.AX = 2
    CALL INTERRUPT(&H33, INREGS, OUTREGS)
    DO
        INREGS.AX = 3
        CALL INTERRUPT(&H33, INREGS, OUTREGS)
    LOOP UNTIL OUTREGS.BX = 0
    LINE (X1% - 1, Y1% - 1)-(X2% + 1, Y2% + 1), 15, B, &H3333
    LOCATE 1, 1
    PRINT SPACE$(39); : LOCATE 1, 1: PRINT "Saving....."
    OPEN FILNAM$ FOR OUTPUT AS #1 'get lowerright into X2%,Y2% and save
    PRINT #1, CHR$(13) + "BHP" + MKI$(X1%) + MKI$(Y1%) + MKI$(X2%) + MKI$(Y2%);
    PUT (0, 8), STORAGE, PSET
    DEF SEG = &HA000
    FOR I% = X1% TO X2%
        FOR J% = Y1% TO Y2%
            PRINT #1, CHR$(PEEK(I% + J% * 320));
        NEXT J%
        BLABLA% = BLABLA% + 1
        LINE (X1% - 1, Y1% - 1)-(X2% + 1, Y2% + 1), 0, B
        SELECT CASE BLABLA%
            CASE 1: LINE (X1% - 1, Y1% - 1)-(X2% + 1, Y2% + 1), 15, B, &H9999
            CASE 2: LINE (X1% - 1, Y1% - 1)-(X2% + 1, Y2% + 1), 15, B, &HCCCC
            CASE 3: LINE (X1% - 1, Y1% - 1)-(X2% + 1, Y2% + 1), 15, B, &H6666
            CASE 4: LINE (X1% - 1, Y1% - 1)-(X2% + 1, Y2% + 1), 15, B, &H3333: BLABLA% = 0
        END SELECT
    NEXT I%
    DEF SEG
    CLOSE #1
    PUT (0, 8), STORAGE, PSET
    MENUBAR
    INREGS.AX = 1
    CALL INTERRUPT(&H33, INREGS, OUTREGS)
    COLOR FORE%
    SAVED = -1
    RETURN
LOADFILE:
    COLOR 15
    SAVED = -1
    GOSUB PREPARE
    INREGS.AX = 2
    CALL INTERRUPT(&H33, INREGS, OUTREGS)
    TEXT$ = "ษออออออออออออออออออออป"
    ROW = 11
    GOSUB CENTER
    TEXT$ = "บ Filename (no ext)  บ"
    ROW = 12
    GOSUB CENTER
    TEXT$ = "บ :                  บ"
    ROW = 13
    GOSUB CENTER
    TEXT$ = "ศออออออออออออออออออออผ"
    ROW = 14
    GOSUB CENTER
    LOCATE 13, 13
    LINE INPUT FILNAM$
    PUT (0, 8), STORAGE, PSET
    FILNAM$ = FILNAM$ + ".BHP"
    ON ERROR GOTO NOTEXIST
    OPEN FILNAM$ FOR INPUT AS #1
    IF NOT FLAG THEN CLOSE #1 ELSE FLAG = 0: MENUBAR
    OPEN FILNAM$ FOR BINARY AS #1
    ON ERROR GOTO BADFORMAT
    a$ = INPUT$(4, 1): IF a$ <> CHR$(13) + "BHP" THEN GOTO BADMODE
    IF FLAG THEN RETURN
    X1% = CVI(INPUT$(2, 1))
    IF FLAG THEN RETURN
    Y1% = CVI(INPUT$(2, 1))
    IF FLAG THEN RETURN
    X2% = CVI(INPUT$(2, 1))
    IF FLAG THEN RETURN
    Y2% = CVI(INPUT$(2, 1))
    IF FLAG THEN RETURN
    DEF SEG = &HA000
    IF FLAG THEN DEF SEG : RETURN
    FOR I% = X1% TO X2%
        b$ = INPUT$((Y2% - Y1%) + 1, 1)
        IF FLAG THEN DEF SEG : RETURN
        FOR J% = Y1% TO Y2%
            a$ = MID$(b$, J% - Y1% + 1, 1)
            IF FLAG THEN DEF SEG : RETURN
            IF a$ = CHR$(0) THEN
                POKE I% + J% * 320!, 0
                IF FLAG THEN DEF SEG : RETURN
            ELSE
                POKE I% + J% * 320!, ASC(a$)
                IF FLAG THEN DEF SEG : RETURN
            END IF
        NEXT J%
    NEXT I%
    DEF SEG
    CLOSE #1
    IF FLAG THEN RETURN
    ON ERROR GOTO 0
    GET (0, 8)-(319, 199), STORAGE
    PUT (0, 8), STORAGE, PSET
    MENUBAR
    INREGS.AX = 1
    CALL INTERRUPT(&H33, INREGS, OUTREGS)
    COLOR FORE%
    RETURN
BADFORMAT:
    PUT (0, 8), STORAGE, PSET
    TEXT$ = "ษออออออออออออออออออออป"
    ROW = 11
    GOSUB CENTER
    TEXT$ = "บ Bad Format In File บ"
    ROW = 12
    GOSUB CENTER
    TEXT$ = "ศออออออออออออออออออออผ"
    ROW = 13
    GOSUB CENTER
    SLEEP: WHILE INKEY$ <> "": WEND
    PUT (0, 8), STORAGE, PSET
    INREGS.AX = 1
    CALL INTERRUPT(&H33, INREGS, OUTREGS)
    FLAG = -1
    RESUME NEXT
BADMODE:
    PUT (0, 8), STORAGE, PSET
    TEXT$ = "ษออออออออออออออออออออป"
    ROW = 11
    GOSUB CENTER
    TEXT$ = "บ  Wrong Video Mode  บ"
    ROW = 12
    GOSUB CENTER
    TEXT$ = "ศออออออออออออออออออออผ"
    ROW = 13
    GOSUB CENTER
    SLEEP: WHILE INKEY$ <> "": WEND
    PUT (0, 8), STORAGE, PSET
    INREGS.AX = 1
    CALL INTERRUPT(&H33, INREGS, OUTREGS)
    FLAG = -1
    RESUME NEXT
NOTEXIST:
    PUT (0, 8), STORAGE, PSET
    TEXT$ = "ษออออออออออออออออออออป"
    ROW = 11
    GOSUB CENTER
    TEXT$ = "บ   File Not Found   บ"
    ROW = 12
    GOSUB CENTER
    TEXT$ = "ศออออออออออออออออออออผ"
    ROW = 13
    GOSUB CENTER
    SLEEP: WHILE INKEY$ <> "": WEND
    INREGS.AX = 1
    CALL INTERRUPT(&H33, INREGS, OUTREGS)
    PUT (0, 8), STORAGE, PSET
    FLAG = -1
    RESUME NEXT

SPRAY:
    DATA 15,15,15, 0,15,15,15, 0,15,15,15, 0,15,15,15, 0,15, 0,15, 0
    DATA 15, 0, 0, 0,15, 0,15, 0,15, 0,15, 0,15, 0,15, 0,15, 0,15, 0
    DATA 15,15,15, 0,15,15,15, 0,15,15,15, 0,15,15,15, 0,15,15,15, 0
    DATA  0, 0,15, 0,15, 0, 0, 0,15,15, 0, 0,15, 0,15, 0, 0,15, 0, 0
    DATA 15,15,15, 0,15, 0, 0, 0,15, 0,15, 0,15, 0,15, 0, 0,15, 0, 0

SHADE:
    DATA 15,15,15, 0,15, 0,15, 0,15,15,15, 0,15,15, 0, 0,15,15,15, 0
    DATA 15, 0, 0, 0,15, 0,15, 0,15, 0,15, 0,15, 0,15, 0,15, 0, 0, 0
    DATA 15,15,15, 0,15,15,15, 0,15,15,15, 0,15, 0,15, 0,15,15, 0, 0
    DATA  0, 0,15, 0,15, 0,15, 0,15, 0,15, 0,15, 0,15, 0,15, 0, 0, 0
    DATA 15,15,15, 0,15, 0,15, 0,15, 0,15, 0,15,15, 0, 0,15,15,15, 0

CLRSWITCH:
    DATA 15,15,15, 0,15, 0, 0, 0,15, 0,15, 0,15,15,15, 0,15,15,15, 0,15, 0,15, 0, 0,15, 0, 0
    DATA 15, 0, 0, 0,15, 0, 0, 0,15, 0,15, 0, 0,15, 0, 0,15, 0,15, 0,15, 0,15, 0,15,15,15, 0
    DATA 15,15, 0, 0,15, 0, 0, 0,15, 0,15, 0, 0,15, 0, 0,15, 0, 0, 0,15, 0,15, 0, 0,15, 0, 0
    DATA  0,15,15, 0,15, 0,15, 0,15, 0,15, 0, 0,15, 0, 0,15, 0, 0, 0,15,15,15, 0, 8, 0, 8, 7
    DATA  0, 0,15, 0,15, 0,15, 0,15, 0,15, 0, 0,15, 0, 0,15, 0,15, 0,15, 0,15, 0, 7, 0, 7, 8
    DATA 15,15,15, 0,15,15,15,15,15, 0,15, 0, 0,15, 0, 0,15,15,15, 0,15, 0,15, 0, 7, 0, 7, 7

REM $STATIC
SUB COLORBAR
    SHARED DRAWFLAG%, SPRAYFLAG%, INREGS AS REGTYPE, OUTREGS AS REGTYPE
    SHARED SHADEFLAG%, BASECOLOR%
    INREGS.AX = 2
    CALL INTERRUPT(&H33, INREGS, OUTREGS)
    LINE (1, 32)-(32, 160), 0, BF
    LINE (1, 32)-(32, 160), 15, B
    LINE (1, 32)-(16, 160), 15, B
    LINE (1, 48)-(32, 48), 15
    LINE (1, 64)-(32, 64), 15
    LINE (1, 80)-(32, 80), 15
    LINE (1, 96)-(32, 96), 15
    LINE (1, 112)-(32, 112), 15
    LINE (1, 128)-(32, 128), 15
    LINE (1, 144)-(32, 144), 15
    LINE (1, 160)-(32, 160), 15
    PAINT (2, 33), BASECOLOR%, 15
    PAINT (2, 49), BASECOLOR% + 1, 15
    PAINT (2, 65), BASECOLOR% + 2, 15
    PAINT (2, 81), BASECOLOR% + 3, 15
    PAINT (2, 97), BASECOLOR% + 4, 15
    PAINT (2, 113), BASECOLOR% + 5, 15
    PAINT (2, 129), BASECOLOR% + 6, 15
    PAINT (2, 145), BASECOLOR% + 7, 15
    PAINT (17, 33), BASECOLOR% + 8, 15
    PAINT (17, 49), BASECOLOR% + 9, 15
    PAINT (17, 65), BASECOLOR% + 10, 15
    PAINT (17, 81), BASECOLOR% + 11, 15
    PAINT (17, 97), BASECOLOR% + 12, 15
    PAINT (17, 113), BASECOLOR% + 13, 15
    PAINT (17, 129), BASECOLOR% + 14, 15
    PAINT (17, 145), BASECOLOR% + 15, 15
    IF DRAWFLAG% THEN
        RESTORE SPRAY
        FOR I% = 1 TO 5
            FOR J% = 1 TO 20
                READ a%
                IF SPRAYFLAG% THEN
                    IF a% = 0 THEN a% = 15 ELSE a% = 0
                END IF
                PSET (J% + 5, I% + 170), a%
            NEXT J%
        NEXT I%
        LINE (3, 168)-(28, 177), 15, B
        RESTORE SHADE
        FOR I% = 1 TO 5
            FOR J% = 1 TO 20
                READ a%
                IF SHADEFLAG% AND SPRAYFLAG% THEN
                    IF a% = 0 THEN a% = 15 ELSE a% = 0
                END IF
                PSET (J% + 5, I% + 182), a%
            NEXT J%
        NEXT I%
        LINE (3, 180)-(28, 189), 15, B
    ELSE
        LINE (3, 168)-(28, 177), 0, BF
        LINE (3, 180)-(28, 189), 0, BF
    END IF
    LINE (1, 21)-(32, 30), 15, B
    RESTORE CLRSWITCH
    FOR I% = 1 TO 6
        FOR J% = 1 TO 28
            READ a%
            PSET (J% + 2, I% + 22), a%
        NEXT J%
    NEXT I%
    INREGS.AX = 1
    CALL INTERRUPT(&H33, INREGS, OUTREGS)
END SUB

SUB INITIALIZE
    SCREEN 8
    LOCATE 1, 1
    FORE% = 15
    BACK% = 0
    STARTUP% = 1
    MENUBAR
    STARTUP% = 0
    FOR I% = 0 TO 319
    FOR J% = 0 TO 7
    THEBAR%(J%, I%) = POINT(I% * 2, J%)
    NEXT J%, I%
    SCREEN 13
    DRAWFLAG% = -1
    ONLY1% = -1
    MENUBAR
    COLORBAR
    INREGS.AX = 0
    CALL INTERRUPT(&H33, INREGS, OUTREGS)
    IF OUTREGS.AX = 0 THEN
        SCREEN 0: WIDTH 80, 25
        PRINT "Error: Paint2 requires a mouse driver."
        SYSTEM
    END IF
    INREGS.AX = 1
    CALL INTERRUPT(&H33, INREGS, OUTREGS)
    DRAWFLAG% = -1
    LINEFLAG% = 0
    BOXFLAG% = 0
    CIRCLEFLAG% = 0
    PAINTFLAG% = 0
    BRUSHSIZE% = 15
    SPEED% = 1
    SAVED% = -1
END SUB

SUB MENUBAR
    SHARED FORE%, BACK%, INREGS AS REGTYPE, OUTREGS AS REGTYPE, DRAWFLAG%
    SHARED LINEFLAG%, BOXFLAG%, CIRCLEFLAG%, PAINTFLAG%, STARTUP%, CLRONLY%
    SHARED ONLY1%, ONLY2%
    IF STARTUP% = 1 THEN
        LOCATE 1, 1
        COLOR 1, 0
        PRINT "EXIT    COLOR   BKGRND  DRAW    LINE    BOX     CIRCLE  PAINT    SAVE    LOAD   ";
        LOCATE 1, 9
        COLOR FORE%, 0
        PRINT "COLOR"
        LOCATE 1, 17
        PRINT "BKGRND"
        COLOR 9, 0
        IF DRAWFLAG% THEN LOCATE 1, 25: PRINT "DRAW"
        IF LINEFLAG% THEN LOCATE 1, 33: PRINT "LINE"
        IF BOXFLAG% THEN LOCATE 1, 41: PRINT "BOX"
        IF CIRCLEFLAG% THEN LOCATE 1, 49: PRINT "CIRCLE"
        IF PAINTFLAG% THEN LOCATE 1, 57: PRINT "PAINT"
        COLOR FORE%, 0
    ELSE
        INREGS.AX = 2
        CALL INTERRUPT(&H33, INREGS, OUTREGS)
        FOR I% = 0 TO 319
            FOR J% = 0 TO 7
                IF I% > 31 AND I% < 64 THEN
                    PSET (I%, J%), ABS(THEBAR%(J%, I%) <> 0) * FORE%
                ELSEIF I% > 63 AND I% < 96 THEN
                    PSET (I%, J%), ABS(THEBAR%(J%, I%) <> 0) * BACK%
                ELSEIF I% > 95 AND I% < 128 THEN
                    IF (ONLY1% = -1) OR (ONLY1% = 1) OR (ONLY2% = 1) THEN PSET (I%, J%), ABS(THEBAR%(J%, I%) <> 0) * (1 + 8 * ABS(DRAWFLAG%))
                ELSEIF I% > 127 AND I% < 160 THEN
                    IF (ONLY1% = -1) OR (ONLY1% = 2) OR (ONLY2% = 2) THEN PSET (I%, J%), ABS(THEBAR%(J%, I%) <> 0) * (1 + 8 * ABS(LINEFLAG%))
                ELSEIF I% > 159 AND I% < 192 THEN
                    IF (ONLY1% = -1) OR (ONLY1% = 3) OR (ONLY2% = 3) THEN PSET (I%, J%), ABS(THEBAR%(J%, I%) <> 0) * (1 + 8 * ABS(BOXFLAG%))
                ELSEIF I% > 191 AND I% < 224 THEN
                    IF (ONLY1% = -1) OR (ONLY1% = 4) OR (ONLY2% = 4) THEN PSET (I%, J%), ABS(THEBAR%(J%, I%) <> 0) * (1 + 8 * ABS(CIRCLEFLAG%))
                ELSEIF I% > 223 AND I% < 256 THEN
                    IF (ONLY1% = -1) OR (ONLY1% = 5) OR (ONLY2% = 5) THEN PSET (I%, J%), ABS(THEBAR%(J%, I%) <> 0) * (1 + 8 * ABS(PAINTFLAG%))
                ELSE
                    IF (NOT CLRONLY%) OR (ONLY1% = -1) OR (ONLY2% = -1) THEN PSET (I%, J%), THEBAR%(J%, I%)
                END IF
            NEXT J%
        NEXT I%
        INREGS.AX = 1
        CALL INTERRUPT(&H33, INREGS, OUTREGS)
    END IF
END SUB

