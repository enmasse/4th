\ Test [IF] skipping with colon definitions containing IF
\ This reproduces the error from paranoia.4th

\ First, define [UNDEFINED] if it doesn't exist
: [UNDEFINED]  ( "name" -- flag )
  bl word find nip 0= ; immediate

\ Now test: F= should NOT be defined, so this block should execute
[UNDEFINED] F= [IF]
  \ This should define F=
  : F= ( F: r1 r2 -- ) ( -- flag )
    FDUP F0= IF FABS THEN  FSWAP
    FDUP F0= IF FABS THEN
    0E F~ ;
[THEN]

\ If we got here without error, the test passed
." Test passed!" CR
