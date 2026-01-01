\ Test case with F= already defined

\ Define F= first
: F= ( F: r1 r2 -- ) ( -- flag )
    FDUP F0= IF FABS THEN FSWAP  
    FDUP F0= IF FABS THEN 
    0E F~ ;

CR .( F= is now defined) CR

\ Now try to redefine it conditionally (should skip)
[UNDEFINED] F= [IF]
    CR .( ERROR: This should not print!) CR
    : F= ( F: r1 r2 -- ) ( -- flag )
        FDUP F0= IF FABS THEN FSWAP  
        FDUP F0= IF FABS THEN 
        0E F~ ;
[THEN]

CR .( Done) CR
