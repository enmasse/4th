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

\ Right-justified numeric output
\ .R ( n width -- ) - print number n right-justified in width characters
: .R >R DUP ABS 0 <# #S ROT SIGN #> R> OVER - SPACES TYPE ;

\ Comments for documentation
( Stack: a b c -- b c a )
\ ROT is primitive

( Stack: a -- a a | a if non-zero )
\ ?DUP duplicates only if non-zero

( Stack: n lo hi -- flag )
\ WITHIN tests if lo <= n < hi
\ Example: 5 3 10 WITHIN gives true (non-zero)
