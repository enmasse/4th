\ Test file for paranoia IF issue

CR .( Starting paranoia IF test... ) CR

\ Step 1: Test [UNDEFINED] F= pattern  
CR .( Step 1: Testing [UNDEFINED] F= ) CR
[UNDEFINED] F= CR .( Result: ) .S CR

\ Step 2: Try the actual pattern from paranoia
CR .( Step 2: Defining F= if undefined ) CR
[UNDEFINED] F= [IF]
	CR .( Inside [IF] block, about to define F=... ) CR
	: F= ( F: r1 r2 -- ) ( -- flag )
	    FDUP   F0= IF FABS THEN  FSWAP  
	    FDUP   F0= IF FABS THEN 
	    0E F~ ;
	CR .( F= defined ) CR
[THEN]

CR .( Step 3: Done ) CR
