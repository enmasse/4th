\ Forth.Core Standard Prelude
\ Pure Forth definitions of convenience words
\ This file is automatically loaded after core primitives

\ Boolean constants
: TRUE -1 ;
: FALSE 0 ;

\ Logical operations
: NOT 0= ;

\ Output formatting
: SPACE 32 EMIT ;
: SPACES DUP 0 > IF 0 DO SPACE LOOP ELSE DROP THEN ;

\ Unsigned output (using pictured numeric)
: U. <# #S #> TYPE SPACE ;

\ Floating-point special constants
\ These words define floating-point special values used by the test suite
\ Using Forth shorthand notation: 1e = 1.0, 0e = 0.0
: +Inf 1e 0e F/ ;   \ Positive infinity
: -Inf -1e 0e F/ FNEGATE ;  \ Negative infinity (negate after division)
: NaN 0e 0e F/ ;     \ Not-a-Number

\ Comments for documentation
( Stack: a b c -- b c a )
\ ROT is primitive

( Stack: a -- a a | a if non-zero )
\ ?DUP duplicates only if non-zero

( Stack: n lo hi -- flag )
\ WITHIN tests if lo <= n < hi
\ Example: 5 3 10 WITHIN gives true (non-zero)
