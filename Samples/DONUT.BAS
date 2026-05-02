' Translated from donut.c by Andy Sloane:
'
'             k;double sin()
'         ,cos();main(){float A=
'       0,B=0,i,j,z[1760];char b[
'     1760];printf("\x1b[2J");for(;;
'  ){memset(b,32,1760);memset(z,0,7040)
'  ;for(j=0;6.28>j;j+=0.07)for(i=0;6.28
' >i;i+=0.02){float c=sin(i),d=cos(j),e=
' sin(A),f=sin(j),g=cos(A),h=d+2,D=1/(c*
' h*e+f*g+5),l=cos      (i),m=cos(B),n=s\
'in(B),t=c*h*g-f*        e;int x=40+30*D*
'(l*h*m-t*n),y=            12+15*D*(l*h*n
'+t*m),o=x+80*y,          N=8*((f*e-c*d*g
' )*m-c*d*e-f*g-l        *d*n);if(22>y&&
' y>0&&x>0&&80>x&&D>z[o]){z[o]=D;;;b[o]=
' ".,-~:;=!*#$@"[N>0?N:0];}}/*#****!!-*/
'  printf("\x1b[H");for(k=0;1761>k;k++)
'   putchar(k%80?b[k]:10);A+=0.04;B+=
'     0.02;}}/*****####*******!!=;:~
'       ~::==!!!**********!!!==::-
'         .,~~;;;========;;;:~-.
'             ..,--------,*/

'$STATIC

A# = 0
B# = 0

DIM z#(0 TO 1919)

CLS

p$ = "............,-~:;=!*#$@"

DO
  buf$ = SPACE$(1920)
  ERASE z#

  cA# = COS(A#): sA# = SIN(A#)
  cB# = COS(B#): sB# = SIN(B#)

  FOR j# = 0 TO 6.28 STEP .07
    cj# = COS(j#): sj# = SIN(j#)
    h# = cj# + 2#
    cAsj# = cA# * sj#
    FOR i# = 0 TO 6.28 STEP .04
      ci# = COS(i#): si# = SIN(i#)
      sih# = si# * h#
      cih# = ci# * h#
      D# = 1# / (cAsj# + sA# * sih# + 5#)
      t# = cA# * sih# - sA# * sj#
      x% = 40 + 30# * D# * (cB# * cih# - sB# * t#)
      y% = 12 + 15# * D# * (sB# * cih# + cB# * t#)
      o% = x% + 80 * y% + 1
      N% = 8 * (cB# * (sA# * sj# - cA# * si# * cj#) - sB# * ci# * cj# - cAsj# - sA# * si# * cj#)

      IF D# > z#(o%) THEN
        z#(o%) = D#
        MID$(buf$, o%, 1) = MID$(p$, N% + 12, 1)
      END IF
    NEXT i#
  NEXT j#

  LOCATE 1, 1
  PRINT buf$

  A# = A# + .04#
  B# = B# + .02#
LOOP WHILE INKEY$ = ""
