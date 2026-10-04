SCREEN 13

DEF SEG = &HA000

FOR o& = 0 TO 63999
  POKE o&, 1 - INT(RND * 1.96)
NEXT o&

DIM h%(16)

ibit% = 1
obit% = 2

DO
  OUT &H3C8, 0

  FOR i% = 0 TO 3
    IF i% AND ibit% THEN
      OUT &H3C9, 27
      OUT &H3C9, 34
      OUT &H3C9, 63
    ELSE
      OUT &H3C9, 10
      OUT &H3C9, 17
      OUT &H3C9, 58
    END IF
  NEXT i%

  FOR i% = 0 TO 16
    h%(i%) = i% \ ibit%
  NEXT i%

  m11% = 0
  m12% = 0
  m21% = 0

  FOR o& = 0 TO 320
    IF o& >= 320 THEN m12% = PEEK(o& - 320) AND ibit%
    IF o& >= 319 THEN m13% = PEEK(o& - 319) AND ibit%
    IF o& >= 1 THEN m21% = PEEK(o& - 1) AND ibit%
    m23% = PEEK(o& + 1) AND ibit%
    m31% = PEEK(o& + 319) AND ibit%
    m32% = PEEK(o& + 320) AND ibit%
    m33% = PEEK(o& + 321) AND ibit%

    m22% = PEEK(o&) AND ibit%

    n% = h%(m11% + m12% + m13% + m21% + m23% + m31% + m32% + m33%)

    IF m22% THEN
      IF n% >= 4 THEN m22% = m22% OR obit%
    ELSE
      IF n% >= 5 THEN m22% = m22% OR obit%
    END IF

    POKE o&, m22%
  NEXT o&

  FOR o& = 321 TO 63999
    m11% = PEEK(o& - 321) AND ibit%
    m12% = PEEK(o& - 320) AND ibit%
    m13% = PEEK(o& - 319) AND ibit%
    m21% = PEEK(o& - 1) AND ibit%
    m23% = PEEK(o& + 1) AND ibit%
    m31% = PEEK(o& + 319) AND ibit%
    m32% = PEEK(o& + 320) AND ibit%
    m33% = PEEK(o& + 321) AND ibit%

    m22% = PEEK(o&) AND ibit%

    n% = h%(m11% + m12% + m13% + m21% + m23% + m31% + m32% + m33%)

    IF m22% THEN
      IF n% >= 4 THEN m22% = m22% OR obit%
    ELSE
      IF n% >= 5 THEN m22% = m22% OR obit%
    END IF

    POKE o&, m22%
  NEXT o&

  SWAP ibit%, obit%
LOOP WHILE INKEY$ = ""
