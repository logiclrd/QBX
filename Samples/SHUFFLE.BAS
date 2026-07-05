' This sample program demonstrates that it is easy to get
' a simple shuffle algorithm wrong. Naively, you might
' iterate through the items and move each one to a random
' position -- but if you do this, the fact that a later
' item might also pick that position and move the item a
' second time shifts the probabilities and makes them
' uneven.
'
' To get an even shuffle, once an item has been moved to
' its randomly-chosen position, that item needs to be
' removed from consideration. It's tricky to randomly
' exclude discontinuous indices all over the array, so
' instead you can invert the problem: Instead if picking
' a random location for each item in turn, you can pick
' a random item for each location in turn.
'
' This sample visualizes the bias. When run, initially,
' it is using the shuffle algorithm described in the
' first paragraph. The accumulated results average out
' to a curve, showing that larger values are slightly
' more likely to end up at the start of the array.
'
' Press space to switch to an unbiased algorithm, and
' this time the blue graph evens out quickly to a flat
' line.

SCREEN 13

DIM n%(1 TO 50), m&(1 TO 50), o%

usebrokenshuffle% = -1

ESC$ = CHR$(27)

LOCATE 10, 1
PRINT "Broken"

DO
  FOR i% = 1 TO 50
    n%(i%) = i%
  NEXT i%

  IF usebrokenshuffle% THEN
    FOR i% = 1 TO 50
      SWAP n%(i%), n%(INT(RND * 50) + 1)
    NEXT i%
  ELSE
    FOR i% = 1 TO 50
      SWAP n%(i%), n%(INT(RND * (50 - i% + 1)) + i%)
    NEXT i%
  END IF

  o% = o% + 1

  FOR i% = 1 TO 50
    m&(i%) = m&(i%) + n%(i%)

    LINE (i%, 1)-(i%, n%(i%) - 1), 0
    LINE (i%, n%(i%))-(i%, 50), 12

    m% = m&(i%) \ o%
    j% = i% + 60

    LINE (j%, 1)-(j%, m% - 1), 0
    LINE (j%, m%)-(j%, 50), 9
  NEXT i%

  k$ = INKEY$

  IF k$ = " " THEN
    usebrokenshuffle% = NOT usebrokenshuffle%

    LOCATE 10, 1
    IF usebrokenshuffle% THEN PRINT "Broken" ELSE PRINT "Fixed "

    o% = 0

    FOR i% = 1 TO 50
      m&(i%) = 0
    NEXT i%
  END IF
LOOP WHILE k$ <> ESC$

