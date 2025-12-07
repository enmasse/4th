\ Diagnostic test to find where paranoia.4th fails
\ This mimics the exact patterns from paranoia.4th

CR .( === PARANOIA DIAGNOSTIC TEST ===) CR
CR .( Testing patterns from paranoia.4th line by line...) CR

\ Pattern 1: Basic [UNDEFINED] with [IF] (lines 278-283)
CR .( Pattern 1: [UNDEFINED] with [IF] for word definition ) CR
[UNDEFINED] TESTWORD1 [IF]
  : TESTWORD1 42 ;
[THEN]
CR .( Pattern 1: PASS) CR

\ Pattern 2: [UNDEFINED] F= pattern with IF inside colon def (lines 308-313)
CR .( Pattern 2: Define F= with IF inside ) CR
[UNDEFINED] F=_TEST [IF]
	: F=_TEST ( F: r1 r2 -- ) ( -- flag )
	    FDUP F0= IF FABS THEN FSWAP  
	    FDUP F0= IF FABS THEN 
	    0E F~ ;
[THEN]
CR .( Pattern 2: PASS) CR

\ Pattern 3: Multiple [UNDEFINED] with or (lines 291-297)
CR .( Pattern 3: Multiple [UNDEFINED] with boolean operations ) CR
[UNDEFINED] FAKE1 
[UNDEFINED] FAKE2 
or
[UNDEFINED] FAKE3 
or
[IF]
	CR .( Inside multi-UNDEFINED [IF] block ) CR
[THEN]
CR .( Pattern 3: PASS) CR

\ Pattern 4: [UNDEFINED] F> with IF in colon def (line 315-318)
CR .( Pattern 4: Define F> with IF in body ) CR
[UNDEFINED] F>_TEST [IF] 
	: F>_TEST ( F: r1 r2 -- ) ( -- flag )   
	    FOVER FOVER F< >R F= R> or invert ; 
[THEN]
CR .( Pattern 4: PASS) CR

\ Pattern 5: Nested IF inside [IF] block
CR .( Pattern 5: Nested IF patterns ) CR
[UNDEFINED] NESTED_TEST [IF]
	: NESTED_TEST
		1 2 > IF
			." TRUE BRANCH" CR
		ELSE
			." FALSE BRANCH" CR
		THEN
	;
[THEN]
CR .( Pattern 5: PASS) CR

\ Pattern 6: Test actual F= definition if it exists
CR .( Pattern 6: Check if F= is already defined ) CR
[UNDEFINED] F= .S CR
CR .( If 0 on stack, F= is defined; if -1, undefined ) CR

CR .( === ALL PATTERNS PASSED ===) CR
CR .( The error must be in a more complex interaction ) CR
