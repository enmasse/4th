\ Test case to reproduce paranoia IF issue

CR .( Test: Define F= with IF inside) CR

[UNDEFINED] F= [IF]
	CR .( Defining F=...) CR
	: F= ( F: r1 r2 -- ) ( -- flag )
	    FDUP   F0= IF FABS THEN  FSWAP  
	    FDUP   F0= IF FABS THEN 
	    0E F~ ;
	CR .( F= defined successfully) CR
[THEN]

CR .( Test complete) CR
