\ Forth.Core Standard Prelude
\ Pure Forth definitions of convenience words
\ This file is automatically loaded after core primitives

\ Boolean constants
: TRUE -1 ;
: FALSE 0 ;

\ Logical operations
: NOT 0= ;

\ Extended stack manipulation
: 2DROP DROP DROP ;
: ?DUP DUP IF DUP THEN ;
: NIP SWAP DROP ;
: TUCK SWAP OVER ;

\ Arithmetic convenience
: 1+ 1 + ;
: 1- 1 - ;
: 2* 2 * ;
: 2/ 2 / ;
: ABS DUP 0 < IF NEGATE THEN ;

\ Extended comparisons (using primitives <, =, >)
\ Note: 0=, 0<>, <=, >= remain in C# for now as they're commonly used

\ Output formatting
: SPACE 32 EMIT ;
: SPACES DUP 0 > IF 0 DO SPACE LOOP ELSE DROP THEN ;

\ Memory operations
: 2@ DUP @ SWAP 1 + @ ;
: 2! SWAP OVER 1 + ! ! ;

\ Unsigned output (using pictured numeric)
: U. <# #S #> TYPE SPACE ;

\ Comments for documentation
( Stack: a b c -- b c a )
\ ROT is primitive

( Stack: a -- a a | a if non-zero )
\ ?DUP duplicates only if non-zero
