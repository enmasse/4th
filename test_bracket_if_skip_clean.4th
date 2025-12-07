\ Test case with F= already defined - no .( to avoid tokenizer side effects

\ Define F= first
: F= ( F: r1 r2 -- ) ( -- flag )
    FDUP F0= IF FABS THEN FSWAP  
    FDUP F0= IF FABS THEN 
    0E F~ ;

\ Now try to redefine it conditionally (should skip)
[UNDEFINED] F= [IF]
    : F= ( F: r1 r2 -- ) ( -- flag )
        FDUP F0= IF FABS THEN FSWAP  
        FDUP F0= IF FABS THEN 
        0E F~ ;
[THEN]

\ Test that the original F= still works
1.0e 1.0e F= . CR
