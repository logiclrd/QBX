'$DYNAMIC

CONST PI# = 3.14159265358979

DIM pattern%(1, 1)

SCREEN 12

direction% = 1

border% = 15

RANDOMIZE 1

DO
  CLS
  COLOR border%

  r! = 70 + RND * 10

  WHILE r! < 250
    SELECT CASE FIX(RND * 5)
      CASE 0: RESTORE 10
      CASE 1: RESTORE 20
      CASE 2: RESTORE 30
      CASE 3: RESTORE 40
      CASE 4: RESTORE 50
    END SELECT

    READ patternwidth%, patternheight%

    REDIM pattern%(patternwidth% + 1, patternheight% + 1)

    FOR y% = 1 TO patternheight%
      READ patternline$

      FOR x% = 0 TO LEN(patternline$) - 1
        pattern%(x%, y%) = (MID$(patternline$, x% + 1, 1) = "x")
      NEXT x%
    NEXT y%

    r2! = r! * (1 + (.25 + RND * .5) * patternheight% / patternwidth%)

    repeats% = INT((1 + RND) * (r! / patternwidth% / 2) + 4)

    da# = direction% * (2 * PI#) / (repeats% * patternwidth%)

    FOR repeat% = 1 TO repeats%
      st! = TIMER

      a1# = direction% * repeat% * (2 * PI#) / repeats%

      WHILE a1# < 0: a1# = a1# + 2 * PI#: WEND

      FOR y% = 1 TO patternheight%
        yr1! = r! + (y% - 1) * (r2! - r!) / patternheight%
        yr2! = r! + y% * (r2! - r!) / patternheight%

        a# = a1#

        FOR x% = 1 TO patternwidth%
          a2# = a# + da#

          WHILE a2# < 0: a2# = a2# + 2 * PI#: WEND

          ca# = COS(a#): sa# = SIN(a#)
          ca2# = COS(a2#): sa2# = SIN(a2#)

          IF pattern%(x%, y%) THEN
            IF NOT pattern%(x% - 1, y%) THEN
              LINE (320 + yr1! * ca#, 240 - yr1! * sa#)-(320 + yr2! * ca#, 240 - yr2! * sa#)
            END IF
            IF NOT pattern%(x% + 1, y%) THEN
              LINE (320 + yr1! * ca2#, 240 - yr1! * sa2#)-(320 + yr2! * ca2#, 240 - yr2! * sa2#)
            END IF
          END IF

          IF direction% < 0 THEN SWAP a#, a2#

          IF pattern%(x%, y%) THEN
            IF NOT pattern%(x%, y% - 1) THEN
              CIRCLE (320, 240), yr1!, , a#, a2#
            END IF
            IF NOT pattern%(x%, y% + 1) THEN
              CIRCLE (320, 240), yr2!, , a#, a2#
            END IF
          END IF

          IF direction% > 0 THEN a# = a2#
        NEXT x%
      NEXT y%

      WHILE TIMER = st!: WEND
    NEXT repeat%

    c% = FIX(RND * 15) + 1

    FOR repeat% = 1 TO repeats%
      a# = direction% * repeat% * (2 * PI#) / repeats% + da# * .5

      FOR y% = 1 TO patternheight%
        yr! = r! + (y% - .5) * (r2! - r!) / patternheight%

        FOR x% = 1 TO patternwidth%
          IF pattern%(x%, y%) THEN
            fillx% = 320 + yr! * COS(a#)
            filly% = 240 - yr! * SIN(a#)

            IF fillx% >= 0 AND fillx% < 640 THEN
              IF filly% >= 0 AND filly% < 480 THEN
                IF POINT(fillx%, filly%) <> c% THEN
                  PAINT (fillx%, filly%), c%, border%
                END IF
              END IF
            END IF
          END IF

          a# = a# + da#
        NEXT x%
      NEXT y%
    NEXT repeat%

    FOR repeat% = 1 TO 8
      st! = TIMER
      WHILE TIMER = st!: WEND

      IF INKEY$ <> "" THEN EXIT DO
    NEXT repeat%

    r! = r2! * 1.1
    direction% = -direction%
  WEND

  border% = border% XOR 7
LOOP WHILE INKEY$ = ""

10
DATA 8, 7
DATA .xxxxxxx.
DATA .x.....x.
DATA .x.xxx.x.
DATA .x.x.x.x.
DATA .x.x.xxx.
DATA .x.x.....
DATA xx.xxxxxxx

20
DATA 10, 9
DATA .xxxxxxxxx.
DATA .x.......x.
DATA .x.xxxxx.x.
DATA .x.x...x.x.
DATA .x.x.xxxxx.
DATA .x.x.x.x...
DATA xx.x.x.xxxxx
DATA ...x.x.....
DATA xxxx.xxxxxxx

30
DATA 15, 8
DATA .xxxxxxxxxxxxxx.
DATA .x............x.
DATA .x.xxxx.xxxxx.x.
DATA .x.x..x.x...x.x.
DATA .x.x..x.x.xxxxx.
DATA xx.xxxx.x.x.xxxxx
DATA ........x.x.....
DATA xxxxxxxxx.xxxxxxx

40
DATA 6, 9
DATA xxxxxxxx
DATA .......
DATA xxxx.xxx
DATA ...x.x.
DATA .xxxxx.
DATA .x.x...
DATA xx.xxxxx
DATA .......
DATA xxxxxxxx

50
DATA 12, 7
DATA .xxxxxxxxxxx.
DATA .x.........x.
DATA .x.xxx.xxx.x.
DATA .x.x.x.x.x.x.
DATA .xxx.x.x.xxx.
DATA .....x.x.....
DATA xxxxxx.xxxxxxx
