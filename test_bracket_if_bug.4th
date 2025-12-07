\ Test case to reproduce the [IF] bug

\ First, check if F= is defined
[UNDEFINED] F= . CR

\ If F= is undefined (should return -1), define it
[UNDEFINED] F= [IF]
    CR .( Defining F=) CR
    : F= ( F: r1 r2 -- ) ( -- flag )
        FDUP F0= IF FABS THEN FSWAP  
        FDUP F0= IF FABS THEN 
        0E F~ ;
[THEN]

\ Test that F= works
1.0e 1.0e F= . CR
