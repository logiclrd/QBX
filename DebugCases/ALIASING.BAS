DECLARE SUB foo (a$, b$, c$)
a$ = "spoo"
foo MID$(a$, 2, 2), a$, a$

SUB foo (a$, b$, c$)
PRINT a$, b$, c$
b$ = "fork"
PRINT a$, b$, c$
c$ = "knif"
PRINT a$, b$, c$
END SUB
