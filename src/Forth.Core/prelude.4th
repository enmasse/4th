\ Forth.Core Standard Prelude
\ Pure Forth definitions of convenience words
\ This file is automatically loaded after core primitives

\ Boolean constants
: TRUE -1 ;
: FALSE 0 ;

\ Logical operations
: NOT 0= ;

\ Arithmetic convenience (needed by other words)
: 1+ 1 + ;
: 1- 1 - ;
: 2* 2 * ;
: 2/ 2 / ;
: ABS DUP 0 < IF NEGATE THEN ;

\ Extended stack manipulation
: 2DROP DROP DROP ;
: ?DUP DUP IF DUP THEN ;
: NIP SWAP DROP ;
: TUCK SWAP OVER ;

\ Extended arithmetic
\ */MOD: ( n1 n2 n3 -- rem quot ) multiply n1*n2 then divide by n3
: */MOD >R * R> /MOD ;

\ WITHIN: ( n lo hi -- flag ) test if lo <= n < hi
\ Implementation: save hi, check n >= lo, restore and check n < hi, AND results
: WITHIN >R OVER SWAP >= SWAP R> < AND ;

\ CELLS: ( n -- n*cellsize ) convert count to address units (identity on our platform)
: CELLS ;

\ Extended comparisons (using primitives <, =, >)
\ Note: 0=, 0<>, <=, >= remain in C# for now as they're commonly used

\ Output formatting
: SPACE 32 EMIT ;
: SPACES DUP 0 > IF 0 DO SPACE LOOP ELSE DROP THEN ;

\ Memory operations
: 2@ DUP @ SWAP 1+ @ ;
: 2! SWAP OVER 1+ ! ! ;

\ Unsigned output (using pictured numeric)
: U. <# #S #> TYPE SPACE ;

\ Comments for documentation
( Stack: a b c -- b c a )
\ ROT is primitive

( Stack: a -- a a | a if non-zero )
\ ?DUP duplicates only if non-zero

( Stack: n lo hi -- flag )
\ WITHIN tests if lo <= n < hi
\ Example: 5 3 10 WITHIN gives true (non-zero)
