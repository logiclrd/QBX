DEFINT A-Z

DECLARE SUB BeSilent ()
DECLARE SUB FMend ()
DECLARE FUNCTION FMinit ()
DECLARE SUB FMload (fs$, header$)
DECLARE SUB FMnoteoff (chan, ope)
DECLARE SUB FMplayrowfrom (pat$, ofs, effnum)
DECLARE SUB FMtimer ()
DECLARE SUB FMtouch (chan, ope, vol, ins)
DECLARE SUB FMupdate (chan, ope, Period, ins, vol)
DECLARE SUB FMwrite (i, d)
DECLARE FUNCTION MPUDetect ()
DECLARE FUNCTION MPUvol (vol, ins)
DECLARE SUB MPUwrite (b)
DECLARE FUNCTION note2period (note, Oct, ins)
DECLARE FUNCTION VibVal (ind, mode)
DECLARE SUB ConfigureTimerForSongTempo (tempo%)
DECLARE SUB ProgramTimerRate (tickspersecond#)
DECLARE SUB ResetTimerRate ()
DECLARE FUNCTION CEIL# (x#)

'***FM PLAYER BEGIN***
' option
' common

TYPE adlchan
    ' Per row
    n AS INTEGER 'note
    s AS INTEGER 'instrument (s=sample)
    v AS INTEGER 'volume
    c AS INTEGER 'cmd
    i AS INTEGER 'info
    ' Volatiles
    DefD AS INTEGER
    DefE AS INTEGER
    DefH AS INTEGER
    CurInst AS INTEGER
    CurVol AS INTEGER
    Period AS INTEGER
    VibPos AS INTEGER '0..63
    effres AS INTEGER 'Allocated effect number. -1 = background music
    VibEff AS INTEGER '0=nope, 1=was, 2=is
END TYPE
TYPE insdata
    c4spd AS INTEGER
    vol AS INTEGER
    gm AS INTEGER     'gm program (0-127 or 128-..)
    gmadd AS INTEGER  'disposition (semitones)
    gmvol AS INTEGER  'factor for volumes
    gmbank AS INTEGER 'bank
END TYPE
TYPE fmdata
    CurRow AS INTEGER
    CurOrd AS INTEGER
    NeedPat AS INTEGER
    EdPat AS INTEGER
    TickPos AS INTEGER
    SkipRow AS INTEGER
    ordnum AS INTEGER
    insnum AS INTEGER
    patnum AS INTEGER
    patdelay AS INTEGER
    patdelaymax AS INTEGER
    LoopBegin AS INTEGER
    LoopCount AS INTEGER
    patpos AS INTEGER 'Offset within fnpattern$
    pperror AS DOUBLE
    effectA AS INTEGER
    effectT AS INTEGER
    effind AS INTEGER
    effpptr AS LONG 'Patternpointertablepointer in effectfile
END TYPE
TYPE fmeffdata
    effA AS INTEGER 'Axx for effects, distinct
    effptr AS INTEGER 'Offset within fmeffect$
    effTickPos AS INTEGER
END TYPE
DIM SHARED fmperiod(0 TO 11) AS DOUBLE
DIM SHARED order(0 TO 255)
DIM SHARED adldata(1 TO 100, 0 TO 11)
DIM SHARED insdata(1 TO 100) AS insdata
DIM SHARED adlchan(0 TO 31) AS adlchan
DIM SHARED MIDInote(0 TO 31) AS INTEGER, MIDIchan(0 TO 31) AS INTEGER
DIM SHARED fmfilename$, fmpattern$, fmeffect$(0 TO 3)
DIM SHARED fmdata AS fmdata, fmeff(0 TO 3) AS fmeffdata

CONST FALSE = 0
CONST TRUE = NOT FALSE
CONST MPUBASE = &H330
CONST OPLBase = &H388
CONST efffile = "effects.s3m"

CLS

IF FMinit < 0 THEN PRINT "FM sound not available.": END

' Written 2000-12-31 by Bisqwit (http://iki.fi/bisqwit/)
' Copyright (C) 1992,2004
' Home page: http://bisqwit.iki.fi/source/fmengineqb.html
'
' Reworked to direct TIMER-based timing by Jonathan Gilbert

PRINT MPUDetect

'MPUwrite &HC0
'MPUwrite 0
'MPUwrite 1
'MPUwrite &H90
'MPUwrite 61
'MPUwrite 127

DIM files$(25)

numfiles = 0

files$(numfiles) = DIR$("*.s3m")

DO WHILE files$(numfiles) <> ""
  IF LCASE$(files$(numfiles)) <> "effects.s3m" THEN numfiles = numfiles + 1

  IF numfiles > UBOUND(files$) THEN EXIT DO

  files$(numfiles) = DIR$
LOOP

LOCATE 2, 64
PRINT "(a-"; CHR$(96 + numfiles); ")";
LOCATE 3, 1

statuschar$ = "."

LastTi! = TIMER

DO
    ThisSecond& = LastTi! * (18.2065 * 2.5) / fmdata.effectT / timerscale%

    IF ThisSecond& <> LastSecond& THEN
        PRINT statuschar$;

        IF POS(0) = 80 THEN
            LOCATE , 1
            IF statuschar$ = "*" THEN statuschar$ = "|" ELSE statuschar$ = "*"
        END IF

        LastSecond& = ThisSecond&
    END IF

    Ti! = TIMER
    WHILE Ti! = LastTi!
        y$ = INKEY$
        IF y$ = "0" THEN FMplayeffect 0
        IF y$ = "1" THEN FMplayeffect 1

        IF y$ = " " THEN ispaused = NOT ispaused
        IF y$ = "." THEN CALL BeSilent
        IF y$ = CHR$(27) THEN EXIT DO

        IF LEN(y$) THEN
            fileindex = ASC(y$) - ASC("a")

            IF fileindex >= 0 AND fileindex < numfiles THEN
                FMload files$(fileindex), y$ + ": "
                ispaused = 0
            END IF
        END IF
        Ti! = TIMER
    WEND

    LastTi! = Ti!

    timertick% = timertick% + 1

    IF timertick% >= timerscale% THEN
        timertick% = 0
        IF NOT ispaused THEN FMtimer
    END IF
LOOP

ResetTimerRate

CALL FMend

END

SUB BeSilent
    FOR j = 0 TO 31
        FMnoteoff j, j
    NEXT
    FOR j = 0 TO 244: FMwrite j, 0: NEXT
END SUB

FUNCTION CEIL# (x#)
' We need the ceiling of multiple#. QuickBASIC does
' not have a ceiling function, but it does have a floor
' function, called INT. INT(2.5) will give us 2 where
' we need 3, but if we negate multiple# first, then
' INT(-2.5) will give us -3, which we can then negate
' again to get back a positive value.
CEIL# = -INT(-x#)
END FUNCTION

SUB ConfigureTimerForSongTempo (tempo%)
' S3M tempo is expressed in huamn-centric BPM, but it's really
' configuring the number of ticks (frames) per second. The conversion
' is chosen so that the default tempo of 125 BPM corresponds
' exactly to the PAL vertical refresh rate of 50 Hz (which
' itself comes from the European power grid).
ProgramTimerRate tempo% * .4
END SUB

SUB FMend
    PLAY OFF
    CALL BeSilent
END SUB

FUNCTION FMinit
    ' Disable timers, reset IRQ flags, latch status flags (should be 0)
    FMwrite 4, 96 ' Mask timer IRQ flags
    FMwrite 4, 128 ' Clear timer IRQ flags
    expectedclear = INP(OPLBase)

    ' Enable timer, wait for status flag to be set with timeout
    FMwrite 2, 255 ' Timer 1 shortest interval: 80 microseconds
    FMwrite 4, 33 ' Enable timer 1, unmask timer IRQ flags

    FOR tick = 1 TO 2 ' Wait twice in case the partial first tick is very short
        A! = TIMER
        DO
            expectedflagged = INP(OPLBase)
            IF (expectedflagged AND &H40) <> 0 THEN EXIT FOR
        LOOP WHILE TIMER = A!
    NEXT tick

    ' Mask and clear timer IRQ flags again
    FMwrite 4, 96
    FMwrite 4, 128

    IF ((expectedclear AND &HE0)) OR ((expectedflagged AND &HE0) <> &HC0) THEN
        FMinit = -1
        EXIT FUNCTION
    END IF

    A# = 1712 'Base value for period table
    ' 1 / 2^(1/12) = 0.9438743126816934967
    FOR x = 0 TO 11: fmperiod(x) = A#: A# = A# * .943874312681694: NEXT
    CALL BeSilent

    ' Let us load effects.
    FMload efffile, "1: "

    CALL BeSilent
    fmdata.effind = 0
    FOR x = 0 TO 3: fmeff(x).effA = fmdata.effectA: NEXT
    FOR x = 0 TO 31: adlchan(x).effres = -1: NEXT
    FOR x = 1 TO fmdata.insnum
        FOR y = 0 TO 11
            adldata(70 + x, y) = adldata(x, y)
        NEXT
        insdata(70 + x) = insdata(x)
    NEXT
    fmdata.effpptr = &H61 + fmdata.ordnum + fmdata.insnum * 2
    x = MPUDetect
    FMinit = 1
END FUNCTION

SUB FMload (fs$, header$)
    fmfilename$ = ""

    CALL BeSilent

    fp = FREEFILE
    OPEN fs$ FOR BINARY AS #fp
    IF LOF(fp) = 0 THEN
        CLOSE fp
        KILL s$
        PRINT "FILE NOT FOUND ERROR: "; s$
        EXIT SUB
    END IF

    SEEK fp, &H21
    fmdata.ordnum = CVI(INPUT$(2, fp))
    fmdata.insnum = CVI(INPUT$(2, fp))
    fmdata.patnum = CVI(INPUT$(2, fp))

    SEEK fp, &H32
    fmdata.effectA = ASC(INPUT$(1, fp))
    fmdata.effectT = ASC(INPUT$(1, fp))

    ConfigureTimerForSongTempo fmdata.effectT

    SEEK fp, &H61
    FOR x = 1 TO fmdata.ordnum
        order(x - 1) = ASC(INPUT$(1, 1))
    NEXT
    FOR x = 1 TO fmdata.insnum
        SEEK fp, &H61 + fmdata.ordnum + (x - 1) * 2
        insptr = CVI(INPUT$(2, fp))
        SEEK fp, insptr * 16& + 1 + &H10
        FOR y = 0 TO 11
            adldata(x, y) = ASC(INPUT$(1, fp))
        NEXT
        SEEK fp, insptr * 16& + 1 + &H1C
        insdata(x).vol = ASC(INPUT$(1, fp))
        SEEK fp, insptr * 16& + 1 + &H20
        insdata(x).c4spd = CVI(INPUT$(2, fp))
        SEEK fp, insptr * 16& + 1 + &H30
        s$ = INPUT$(28, fp)

        IF LEFT$(s$, 2) = "GM" THEN
            g = 0
        ELSEIF LEFT$(s$, 2) = "GP" THEN
            g = 1
        ELSE
            g = -1
        END IF
        IF g < 0 THEN
            insdata(x).gm = 0
            insdata(x).gmadd = 0
            insdata(x).gmvol = 0
        ELSE
            n = VAL(MID$(s$, 3))
            IF LEFT$(s$, 2) = "GP" THEN
                n1 = n - 35 + 128
            ELSE
                n1 = n - 1
            END IF

            insdata(x).gmadd = 0
            insdata(x).gmvol = 64
            insdata(x).gm = n1
            IF n < 10 THEN c = 1 ELSE IF n < 100 THEN c = 2 ELSE c = 3
            p = 3 + c
            DO WHILE p < LEN(s$)
                c$ = MID$(s$, p, 1)
                IF c$ = "+" OR c$ = "-" THEN
                    p = p + 1
                    n = VAL(MID$(s$, p))
                    IF n < 10 THEN c = 1 ELSE IF n < 100 THEN c = 2 ELSE c = 3
                    IF c$ = "-" THEN n = -n
                    insdata(x).gmadd = n
                    p = p + c
                ELSEIF c$ = "" THEN
                    p = p + 1
                    n = VAL(MID$(s$, p))
                    IF n < 10 THEN c = 1 ELSE IF n < 100 THEN c = 2 ELSE c = 3
                    insdata(x).gmbank = n
                    p = p + c
                ELSEIF c$ = "/" THEN
                    p = p + 1
                    n = VAL(MID$(s$, p))
                    IF n < 10 THEN c = 1 ELSE IF n < 100 THEN c = 2 ELSE c = 3
                    insdata(x).gmvol = n
                    p = p + c
                ELSEIF c$ = "&" THEN
                    p = p + 1
                    n = VAL(MID$(s$, p))
                    IF n < 10 THEN c = 1 ELSE IF n < 100 THEN c = 2 ELSE c = 3
                    p = p + c
                ELSE
                    EXIT DO
                END IF
            LOOP
            ' GM123+4/6: blah blah
        END IF
    NEXT
    FOR x = 0 TO 31
        adlchan(x).CurInst = 0
        adlchan(x).CurVol = 0
        adlchan(x).n = 255
        adlchan(x).s = 0
        adlchan(x).v = 255
        adlchan(x).c = 0
        adlchan(x).i = 0
        MIDIchan(x) = -1
    NEXT

    fmdata.CurRow = 0
    fmdata.CurOrd = 0
    fmdata.SkipRow = 0
    fmdata.NeedPat = TRUE
    fmdata.LoopBegin = 0
    fmdata.LoopCount = 0
    fmdata.TickPos = 0
    fmdata.EdPat = -1

    CLOSE fp

    fmfilename$ = fs$

    cursorx = POS(0)

    LOCATE 1, 64 - LEN(header$)
    PRINT header$; SPACE$(15);
    LOCATE 1, 64
    PRINT fmfilename$;

    LOCATE 3, cursorx
END SUB

SUB FMnoteoff (chan%, ope%)
    FMwrite &HB0 + chan, 0
    FMwrite &H40 + ope, 255
    FMwrite &H43 + ope, 255

    ' Issue MIDI noteoff command
    MPUwrite &H80 + MIDIchan(chan)
    MPUwrite MIDInote(chan)
    MPUwrite 127

    MIDIchan(chan) = -1
END SUB

SUB FMplayeffect (ind)
    PLAY STOP

    fp = FREEFILE
    OPEN efffile FOR BINARY AS #fp

    SEEK fp, fmdata.effpptr + ind * 2
    patptr = CVI(INPUT$(2, fp))
    SEEK fp, patptr * 16& + 1
    patlen = CVI(INPUT$(2, fp)) - 2
    IF patlen > 0 THEN
        fmeffect$(fmdata.effind) = INPUT$(patlen, fp)
    ELSE
        fmeffect$(fmdata.effind) = ""
    END IF
    fmeff(fmdata.effind).effptr = 1
    fmeff(fmdata.effind).effTickPos = 0
    fmdata.effind = (fmdata.effind + 1) AND 3

    CLOSE fp

    PLAY ON
END SUB

SUB FMplayrowfrom (pat$, ofs%, effnum%)
    IF LEN(pat$) = 0 THEN EXIT SUB
    termination = FALSE

    FOR c = 0 TO 31
        IF adlchan(c).effres = effnum THEN
            adlchan(c).n = 255
            adlchan(c).s = 0
            adlchan(c).v = 255
            adlchan(c).c = 0
            adlchan(c).i = 0
        END IF
    NEXT

    DO
        c = ASC(MID$(pat$, ofs, 1))
        n = ofs + 1

        IF c > 0 THEN
            chan = c AND 31

            IF effnum >= 0 THEN
                adlchan(chan).effres = effnum
                'PRINT "effect"; effnum; "active at channel"; chan
            END IF

            IF (c AND 32) THEN
                note = ASC(MID$(pat$, n, 1)): n = n + 1
                ins = ASC(MID$(pat$, n, 1)): n = n + 1
                IF effnum >= 0 OR adlchan(chan).effres < 0 THEN
                    IF effnum >= 0 THEN ins = ins + 70
                    adlchan(chan).n = note
                    adlchan(chan).s = ins
                END IF
            END IF
            IF (c AND 64) THEN
                vol = ASC(MID$(pat$, n, 1)): n = n + 1
                IF effnum >= 0 OR adlchan(chan).effres < 0 THEN
                    adlchan(chan).v = vol
                END IF
            END IF
            IF (c AND 128) THEN
                cmd = ASC(MID$(pat$, n, 1)): n = n + 1
                info = ASC(MID$(pat$, n, 1)): n = n + 1

                IF effnum >= 0 AND cmd = 3 THEN 'C'
                    termination = TRUE
                    adlchan(chan).effres = -1
                    'PRINT "effect terminated at channel"; chan
                ELSE
                    IF effnum >= 0 OR adlchan(chan).effres < 0 THEN
                        adlchan(chan).c = cmd
                        adlchan(chan).i = info
                    END IF

                    IF cmd = 1 THEN fmdata.effectA = adlchan(chan).i  'A'

                    IF info = 20 THEN
                        fmdata.effectT = adlchan(chan).i 'T'

                        ConfigureTimerForSongTempo fmdata.effectT
                    END IF

                    'IF effnum >= 0 THEN
                    '    PRINT "Found Effect Cmd at channel"; chan; ": ";
                    '    PRINT CHR$(adlchan(chan).c OR 64); HEX$(adlchan(chan).i)
                    'END IF
                END IF
            END IF
        END IF
        ofs = n
    LOOP UNTIL c = 0

    IF termination THEN
        ' Terminate this effect!
        pat$ = ""
        ofs = 1
        EXIT SUB
    END IF
END SUB

SUB FMtimer
    ' Exit quickly if adlib has not been recognized
    IF fmperiod(0) = 0 OR LEN(fmfilename$) = 0 THEN EXIT SUB

    cursorx = POS(0)

    ' S3M effectT determines how many frames to handle in 2.5 seconds.
    ' S3M effectA determines, how many frames there are per a row
    ' This function will always process one frame.

    ' The accuracy of PLAY... might not be good. Let us test.

    ' With PLAY, the command "T" determines the number of quarter notes in a minute.
    ' L4 = quarter note. L1 = whole note (four quarter notes).
    ' L64 is a 16th of a quarter note.
    '
    ' According to T, the length of a quarter note is 60/T seconds.
    ' L determines the length of the note such...
    ' that 240/T/L = length of a note in seconds.

    ' frame length      = effectT * 0.4 seconds.
    ' half frame length = effectT * 0.2 seconds.

    ' 240/255/effectT/0.2 = L
    ' 240/255/0.2 = 4.7058823529411764703

    IF fmdata.TickPos = 0 THEN
        IF fmdata.patdelay = 0 THEN
            'PRINT "Ticking CurOrd="; fmdata.CurOrd; ", CurRow="; fmdata.CurRow

            ' We do not keep all the patterns in memory,
            ' due to the limited memory space of QuickBasic.
            ' Instead, we load them from disk when we need them.

            IF fmdata.NeedPat THEN
                c = 0
                DO
                    CurPat = order(fmdata.CurOrd)
                    IF CurPat < 254 THEN EXIT DO
                    fmdata.CurOrd = (fmdata.CurOrd + 1) MOD fmdata.ordnum
                    c = c + 1
                LOOP WHILE c < fmdata.ordnum

                IF CurPat <> fmdata.EdPat THEN
                    fmdata.EdPat = CurPat

                    LOCATE 1, 10: COLOR 1: PRINT "Loading pattern"; CurPat; "(order"; CurOrd; ")"

                    fp = FREEFILE
                    OPEN fmfilename$ FOR BINARY AS #fp

                    SEEK fp, &H61 + fmdata.ordnum + fmdata.insnum * 2 + CurPat * 2
                    patptr = CVI(INPUT$(2, fp))
                    SEEK fp, patptr * 16& + 1
                    patlen = CVI(INPUT$(2, fp)) - 2
                    IF patlen > 0 THEN fmpattern$ = INPUT$(patlen, fp) ELSE fmpattern$ = ""

                    LOCATE 2, 10: COLOR 1: PRINT "Fetched:"; LEN(fmpattern$); "bytes"

                    CLOSE fp
                END IF

                fmdata.CurRow = 0
                fmdata.NeedPat = FALSE
                fmdata.patpos = 1
            END IF

            ' Now fmpattern$ contains the remainder of the pattern.
            ' fmdata.SkipRow contains the number of rows to be skipped (may originate from Cxx)

            WHILE fmdata.SkipRow > 0
                DO
                    n = 1
                    c = ASC(MID$(fmpattern$, fmdata.patpos, 1))
                    IF (c AND 32) THEN n = n + 2
                    IF (c AND 64) THEN n = n + 1
                    IF (c AND 128) THEN n = n + 2
                    fmdata.patpos = fmdata.patpos + n
                LOOP UNTIL c = 0
                fmdata.SkipRow = fmdata.SkipRow - 1
                fmdata.CurRow = fmdata.CurRow + 1
            WEND

            ' Now fmpattern$ points to the beginning of the current row.
            FMplayrowfrom fmpattern$, fmdata.patpos, -1
        END IF
        fmdata.TickPos = fmdata.effectA
    END IF
    FOR x = 0 TO 3
        IF fmeff(x).effTickPos = 0 THEN
            ' Time to fetch a new row for the effect...
            ' TODO: Somehow clean the adlchan table...

            c = fmdata.effectA
            fmdata.effectA = fmeff(x).effA
            FMplayrowfrom fmeffect$(x), fmeff(x).effptr, x
            ' Muuttaa fmdata.effectA:ta
            fmeff(x).effA = fmdata.effectA
            fmdata.effectA = c
            fmeff(x).effTickPos = fmeff(x).effA
            fmeff(x).effTickPos = fmeff(x).effA
        END IF
    NEXT

    loophere = FALSE
    FOR x = -1 TO 3
        IF x >= 0 THEN
            CurTick = fmeff(x).effA - fmeff(x).effTickPos
        ELSE
            CurTick = fmdata.effectA - fmdata.TickPos  '0..(A-1)
        END IF

        FOR c = 0 TO 8
            IF adlchan(c).effres = x THEN
                ' Mark the vibrato obsolete
                IF adlchan(c).VibEff = 2 THEN adlchan(c).VibEff = 1

                chan = c 'FIXME: Take from the headers
                ope = chan
                IF chan > 2 THEN ope = ope + 5
                IF chan > 5 THEN ope = ope + 5

                notedelay = 0
                notecut = -1

                cmd = adlchan(c).c
                info = adlchan(c).i

                IF cmd = 19 THEN      'S'
                    sclass = info \ 16
                    sinfo = info AND 15

                    'Supported S commands:
                    '   SB0..SBF (loop)
                    '   SCx      (notecut)
                    '   SDx      (notedelay)
                    '   SEx      (patterndelay (handled later))
                    'A=10, B=11, jne.  (hex)

                    IF sclass = 11 THEN  'B'
                        IF CurTick = 0 THEN
                            IF sinfo = 0 THEN
                                fmdata.LoopBegin = fmdata.CurRow
                            ELSE
                                IF fmdata.LoopCount = 0 THEN
                                    fmdata.LoopCount = sinfo + 1
                                END IF
                            END IF
                        END IF
                        IF sinfo > 0 THEN loophere = TRUE
                    END IF
                    IF sclass = 12 THEN notecut = sinfo   'C'
                    IF sclass = 13 THEN notedelay = sinfo 'D'
                    IF sclass = 14 THEN  'E'
                        'PRINT fmdata.patdelay;
                        IF CurTick = 0 THEN
                            IF fmdata.patdelay = 0 THEN
                                fmdata.patdelay = sinfo
                                fmdata.patdelaymax = sinfo
                            ELSE
                                fmdata.patdelay = fmdata.patdelay - 1
                            END IF
                        END IF
                    END IF
                END IF
                IF notedelay = CurTick AND (fmdata.patdelay = fmdata.patdelaymax OR CurTick > 0) THEN
                    vol = adlchan(c).v
                    note = adlchan(c).n
                    ins = adlchan(c).s
                    IF ins = 0 THEN
                        ins = adlchan(c).CurInst
                    ELSE
                        adlchan(c).CurInst = ins
                    END IF
                    IF ins > 0 THEN
                        IF vol = 255 THEN
                            vol = insdata(ins).vol
                        ELSE
                            FMtouch chan, ope, vol, ins
                            adlchan(c).CurVol = vol
                        END IF
                        IF note < 255 THEN 'Something indeed mentioned
                            FMnoteoff chan, ope
                            IF note <> 254 THEN  'adlib notecut
                                Oct = note \ 16
                                note = note AND 15

                                adlchan(c).Period = note2period(note, Oct, ins)

                                FMupdate chan, ope, adlchan(c).Period, ins, vol
                                adlchan(c).CurVol = vol
                            END IF
                        END IF
                    END IF
                END IF
                IF adlchan(c).CurInst > 0 THEN ' No command on channels without instrus playing
                    IF cmd = 4 OR cmd = 11 THEN 'D', 'K'
                        vol = adlchan(c).CurVol
                        IF info = 0 THEN info = adlchan(c).DefD ELSE adlchan(c).DefD = info
                        IF info = 255 THEN       'FF
                            vol = 64
                        ELSEIF info >= 240 THEN  'F0..FE
                            IF CurTick = 0 THEN vol = vol - (info AND 15)
                        ELSEIF info < 16 THEN    '01..0F
                            vol = vol - (info AND 15)
                        ELSEIF (info AND 15) = 15 THEN
                            IF CurTick = 0 THEN vol = vol + (info \ 16)
                        ELSEIF info <= 240 THEN '10..F0
                            vol = vol + (info \ 16)
                        END IF
                        IF vol < 0 THEN vol = 0 ELSE IF vol > 64 THEN vol = 64
                        adlchan(c).CurVol = vol
                        FMtouch chan, ope, vol, adlchan(c).CurInst
                    END IF
                    IF cmd = 5 OR cmd = 6 THEN     'E' or 'F'
                        IF info = 0 THEN info = adlchan(c).DefE ELSE adlchan(c).DefE = info
                        IF cmd = 6 THEN info = -info
                        adlchan(c).Period = adlchan(c).Period + info * 4
                        FMupdate chan, ope, adlchan(c).Period, adlchan(c).CurInst, adlchan(c).CurVol
                    END IF
                    IF cmd = 8 OR cmd = 11 THEN     'H', 'K'
                        IF cmd = 11 THEN info = 0
                        IF info = 0 THEN info = adlchan(c).DefH ELSE adlchan(c).DefH = info
                        IF adlchan(c).VibEff = 0 THEN
                            adlchan(c).VibPos = 0
                        END IF
                        vspeed = info \ 16
                        vdepth = info AND 15
                        value = VibVal(adlchan(c).VibPos, 0) * vdepth \ 32
                        FMupdate chan, ope, adlchan(c).Period + value, adlchan(c).CurInst, adlchan(c).CurVol
                        adlchan(c).VibPos = adlchan(c).VibPos + vspeed
                        adlchan(c).VibEff = 2
                    END IF
                    IF notecut = CurTick AND (fmdata.patdelay = fmdata.patdelaymax OR CurTick > 0) THEN
                        FMnoteoff chan, ope
                    END IF
                END IF
                IF adlchan(c).VibEff = 1 THEN 'Vibrato still obsolete?
                    adlchan(c).VibEff = 0   'No more vibrato
                    FMupdate chan, ope, adlchan(c).Period, adlchan(c).CurInst, adlchan(c).CurVol
                END IF
            END IF
        NEXT
    NEXT

    fmdata.TickPos = fmdata.TickPos - 1
    FOR x = 0 TO 3
        fmeff(x).effTickPos = fmeff(x).effTickPos - 1
    NEXT

    ' If this row is finished, find out where to go next.
    IF fmdata.TickPos = 0 AND fmdata.patdelay = 0 THEN
        fmdata.patdelaymax = 0
        brkord = -1
        brkrow = -1

        fmdata.CurRow = fmdata.CurRow + 1
        IF fmdata.CurRow = 64 THEN
            brkrow = 0
        END IF

        doloop = FALSE
        IF fmdata.LoopCount > 0 AND loophere = TRUE THEN
            fmdata.LoopCount = fmdata.LoopCount - 1
            IF fmdata.LoopCount = 0 THEN
                ' SB0 pending.
                fmdata.LoopBegin = fmdata.CurRow
            ELSE
                brkord = fmdata.CurOrd
                brkrow = fmdata.LoopBegin
                doloop = TRUE
            END IF
        END IF

        IF doloop = FALSE THEN
            FOR c = 0 TO 8
                cmd = adlchan(c).c     '1=A, 2=B, and so on... 0=none
                info = adlchan(c).i

                IF cmd = 2 THEN   'B'
                    brkord = info
                    fmdata.LoopBegin = 0
                END IF
                IF cmd = 3 THEN   'C'
                    brkrow = (info \ 16) * 10 + (info AND 15)
                    fmdata.LoopBegin = 0
                END IF
            NEXT
            IF brkord >= 0 AND brkrow = -1 THEN brkrow = 0
        END IF
        IF brkrow >= 0 THEN
            fmdata.CurOrd = (fmdata.CurOrd + 1) MOD fmdata.ordnum
            fmdata.NeedPat = TRUE
            IF brkord >= 0 THEN fmdata.CurOrd = brkord
            fmdata.SkipRow = brkrow
        END IF
    END IF

    LOCATE 3, cursorx
    COLOR 7
END SUB

SUB FMtouch (chan, ope, vol, ins)
    v = vol * 63 \ 64

    c = (adldata(ins, 2) AND &HC0)
    c = c OR (63 + (adldata(ins, 2) AND 63) * v \ 63 - v)
    FMwrite &H40 + ope, c

    c = (adldata(ins, 3) AND &HC0)
    c = c OR (63 + (adldata(ins, 3) AND 63) * v \ 63 - v)
    FMwrite &H43 + ope, c

    MPUwrite &HB0 + MIDIchan(chan)
    MPUwrite 7    'Master volume

    v = MPUvol(vol, ins)
    MPUwrite v
END SUB

SUB FMupdate (chan, ope, Period, ins, vol)
    ' Patch with the instrument data

    IF ins <= 0 THEN EXIT SUB

    FMwrite &H20 + ope, adldata(ins, 0)
    FMwrite &H60 + ope, adldata(ins, 4)
    FMwrite &H80 + ope, adldata(ins, 6)
    FMwrite &HE0 + ope, adldata(ins, 8) AND 3

    FMwrite &H23 + ope, adldata(ins, 1)
    FMwrite &H63 + ope, adldata(ins, 5)
    FMwrite &H83 + ope, adldata(ins, 7)
    FMwrite &HE3 + ope, adldata(ins, 9) AND 3

    FMwrite &HC0 + chan, adldata(ins, 10) OR &H30

    IF insdata(ins).gm < 128 THEN
        IF Period = 0 THEN
            n = 0
        ELSE
            ' Converting a period into MIDI note number:

            ' Hz = fmperiod(0) * 5500& / Period
            ' 2 ^ (notenumber / 12) = Hz
            ' Solve notenumber, get:
            ' notenumber = 12 * log(Hz) / log(2)
            'n = 12 * LOG(fmperiod(0) * 5500& / period) / LOG(2) + insdata(ins).gmadd - 89
            ' Simplify:
            n = insdata(ins).gmadd + CINT(60.10259 + 17.31234 * LOG(fmperiod(0) / Period))
            IF n < 1 THEN n = 1
            IF n > 127 THEN n = 127
        END IF

        IF MIDIchan(chan) = -1 THEN
            MIDIchan(chan) = chan
            MIDInote(chan) = n

            MPUwrite &HC0 + MIDIchan(chan) ' Patch
            MPUwrite insdata(ins).gm

            MPUwrite &H90 + MIDIchan(chan)
            MPUwrite MIDInote(chan)
            MPUwrite 127
        END IF
    ELSE
        IF MIDIchan(chan) = -1 THEN
            MIDIchan(chan) = 9                'Percussion
            MIDInote(chan) = insdata(ins).gm + 35 - 128

            MPUwrite &H90 + MIDIchan(chan)
            MPUwrite MIDInote(chan)
            MPUwrite 127'MPUvol(vol, ins)
        END IF
    END IF

    FMtouch chan, ope, vol, ins

    Oct = 0
    IF Period = 0 THEN
        ' Exception, circumvention of a bug or something
        Herz& = 0
    ELSE
        '5500 = 22000 / 4
        Herz& = fmperiod(0) * 5500& / Period
    END IF

    WHILE Herz& >= 512
        Oct = Oct + 1
        Herz& = Herz& \ 2
    WEND

    FMwrite &HA0 + chan, Herz& AND 255
    FMwrite &HB0 + chan, 32 OR ((Herz& \ 256) AND 3) OR ((Oct AND 7) * 4)
END SUB

SUB FMwrite (i, d)
    OUT OPLBase, i: A = INP(OPLBase): OUT OPLBase + 1, d: A = INP(OPLBase)
END SUB

FUNCTION MPUDetect
    MPUDetect = 0

    OUT MPUBASE + 1, 255
    b = 10000
    WHILE (INP(MPUBASE + 1) AND &H40) <> 0
        b = b - 1
        IF b = 0 THEN EXIT FUNCTION
    WEND
    OUT MPUBASE + 1, 255
    b = 10000
MPULoop:
    WHILE (INP(MPUBASE + 1) AND &H80) <> 0
        b = b - 1
        IF b = 0 THEN EXIT FUNCTION
    WEND

    IF INP(MPUBASE) <> 254 THEN GOTO MPULoop

    MPUDetect = 0

    WHILE (INP(MPUBASE + 1) AND &H40) <> 0: WEND
    OUT MPUBASE + 1, 63
    b = 10000
MPULoop2:
    WHILE (INP(MPUBASE + 1) AND &H80) <> 0
        b = b - 1
        IF b = 0 THEN EXIT FUNCTION
    WEND

    IF INP(MPUBASE) <> 254 THEN GOTO MPULoop2

    MPUDetect = -1
END FUNCTION

FUNCTION MPUvol (vol, ins)
    v = vol * insdata(ins).gmvol \ 64
    IF v > 0 THEN v = EXP(v / (64 / 4)) * 127 / EXP(4)
    IF v < 0 THEN v = 0 ELSE IF v > 127 THEN v = 127
    MPUvol = v
END FUNCTION

SUB MPUwrite (b)
    EXIT SUB 'disabled for now

    WHILE (INP(MPUBASE + 1) AND &H40) <> 0
        GOSUB MPUwritedcheck: IF d& = 0 THEN GOTO MPUwriteesub
    WEND
    OUT MPUBASE, b ' And a little delay then? Nah, QBasic is slow enough.
MPUwriteesub:
    EXIT SUB
MPUwritedcheck:
    d& = d& + 1
    'b# = b# * .0001
    IF d& > 800000 THEN
        d& = MPUDetect
        'COLOR 11
        'PRINT "It appears that your MPU-401 crashed... ";
        IF d& = 1 THEN
dsuccess:   'PRINT "But it woke up again";
            'IF d& > 1 THEN PRINT " (after few tries...)";
            'PRINT
        ELSE
            d& = MPUDetect
            IF d& = 0 THEN d& = MPUDetect
            IF d& = 1 THEN d& = 2: GOTO dsuccess
            'PRINT "Argh"
        END IF
        'COLOR 7
    END IF
RETURN 'This was a GOSUB label
END SUB

FUNCTION note2period (note, Oct, ins)
    c4spd = insdata(ins).c4spd

    tmp& = 8363& * 16& * fmperiod(note)

    IF c4spd = 0 THEN
        ' Exception, circumvention of a bug or something
        note2period = 0
    ELSE
        note2period = (tmp& \ (2 ^ Oct)) / c4spd
    END IF
END FUNCTION

SUB ProgramTimerRate (tickspersecond#)
SHARED timerscale%

CONST BaseFrequency& = 1193181

interval& = BaseFrequency& / tickspersecond#

IF interval& <= 65536 THEN
  ' NB: 65536 itself is okay, because programming a timer
  ' reset value of 0 _is_ effectively 65536, giving a
  ' frequency of about 18.2 Hz.
  timerscale% = 1
ELSE
  ' We need to find an integer multiple that will yield
  ' a frequency whose interval fits into 16 bits. This
  ' means a frequency greater than the minimum frequency,
  ' about 18.2 Hz. In practice, the value needs to be
  ' strictly larger than the minimum, otherwise interval&
  ' will be out of range. 18.2 is less than the exact
  ' value, but 18.21 is larger than it.
  timerscale% = CEIL(18.21 / tickspersecond#)
  interval& = BaseFrequency& / (tickspersecond# * timerscale%)
END IF

OUT &H43, &H36
OUT &H40, interval&
OUT &H40, interval& \ 256
END SUB

DEFSNG A-Z
SUB ResetTimerRate
OUT &H43, &H36
OUT &H40, 0
OUT &H40, 0
END SUB

DEFINT A-Z
FUNCTION VibVal (ind, mode)
    '
    ' The variable pointed by "ind" is MODIFIED in this function
    ' mode... 0..3 = type, [+4] = loop

    IF mode >= 4 THEN
        ind = ind AND 63
    ELSEIF ind >= 64 THEN
        ind = 0
    END IF

    SELECT CASE mode
        CASE 0:
            'VibVal = 255*SIN(ind * 3.1415926535968# / 32)
            VibVal = 255 * SIN(ind * .098174770424681)
            EXIT FUNCTION
        CASE 1:
            IF ind = 0 THEN VibVal = 0 ELSE VibVal = (-256 + ind * 8)
            EXIT FUNCTION
        CASE 2:
            IF ind < 0 THEN VibVal = 0 ELSE VibVal = 255
            EXIT FUNCTION
    END SELECT
    EXIT FUNCTION
END FUNCTION
