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
DIM SHARED MIDInote(0 to 31) AS INTEGER, MIDIchan(0 to 31)AS INTEGER
DIM SHARED fmfilename$, fmpattern$, fmeffect$(0 TO 3)
DIM SHARED fmdata AS fmdata, fmeff(0 TO 3) AS fmeffdata

CONST FALSE = 0
CONST TRUE = NOT FALSE
CONST MPUBASE = &H330
CONST OPLBase = &H388
CONST efffile = "effects.s3m"
'***FM PLAYER END***
