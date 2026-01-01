\ Minimal reproduction of paranoia IF error

\ This pattern should skip the F> definition because F> is already defined
[UNDEFINED] F>  [IF] 
	: F>  ( F: r1 r2 -- ) ( -- flag )   
	    FOVER FOVER F< >R F= R> or invert ; 
[THEN]

CR .( Test complete - if you see this, it worked! ) CR
