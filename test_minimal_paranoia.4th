\ Minimal paranoia.4th reproduction - stress test
\ Testing many [undefined] patterns to trigger synchronization issue

DECIMAL

\ First, define [UNDEFINED] using the EXACT pattern from paranoia.4th
s" [UNDEFINED]" dup pad c! pad char+ swap cmove 
pad find nip 0=
[IF]
: [UNDEFINED]  ( "name" -- flag )
  bl word find nip 0= ; immediate
[THEN]

s" [DEFINED]" dup pad c! pad char+ swap cmove 
pad find nip 0=
[IF]
: [DEFINED]  postpone [UNDEFINED] 0= ; immediate
[THEN]

\ Now stress-test with MANY [undefined] patterns like paranoia does

[UNDEFINED] F~ 
[UNDEFINED] F<  or
[UNDEFINED] F0= or
[UNDEFINED] F** or
[IF]
  ." ** Requires F** and F~ and F< and F0= **" CR ABORT
[THEN]

[UNDEFINED] FS. [IF]
[DEFINED] F. [IF]  
  : FS. F. ; 
[ELSE] 
  ." ** Requires FS. or F. for output **" ABORT
[THEN]
[THEN]

[UNDEFINED] F= [IF]
  : F= ( F: r1 r2 -- ) ( -- flag )
    FDUP F0= IF FABS THEN FSWAP  
    FDUP F0= IF FABS THEN 
    0E F~ ;
[THEN]

[UNDEFINED] F<> [IF] : F<> ( F: r1 r2 -- ) ( -- flag )   F= invert ;  [THEN]

[UNDEFINED] F>  [IF] 
  : F>  ( F: r1 r2 -- ) ( -- flag )   
    FOVER FOVER F< >R F= R> or invert ; 
[THEN]

[UNDEFINED] F<= [IF] : F<= ( F: r1 r2 -- ) ( -- flag )   F> invert ;  [THEN]
[UNDEFINED] F>= [IF] : F>= ( F: r1 r2 -- ) ( -- flag )   F< invert ;  [THEN]
[UNDEFINED] S>F [IF] : S>F ( n -- ) ( F: -- r )  S>D D>F ;  [THEN]

[UNDEFINED] <=     [IF] : <=  ( n1 n2 -- flag )  2DUP < >R = R> OR ;  [THEN]
[UNDEFINED] ?allot [IF] : ?allot ( u -- a ) HERE SWAP ALLOT ; [THEN]
[UNDEFINED] cell-  [IF] : cell- ( a1 -- a2 ) 1 CELLS - ;      [THEN]

." If you see this, the test passed!" CR
BYE
