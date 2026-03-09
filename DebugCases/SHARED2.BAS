DIM SHARED a%

a% = 35

PRINT a%

foo

PRINT a%

SUB foo
	SHARED a%

	a% = 42
END SUB
