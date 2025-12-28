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

FVARIABLE +Inf
FVARIABLE -Inf
FVARIABLE NaN

\ Initialize special values
1.0d 0.0d F/ +Inf F!
-1.0d 0.0d F/ -Inf F!
0.0d 0.0d F/ NaN F!

\ Mathematical constants
3.141592653589793d CONSTANT pi

\ Conditional compilation helper
\ [UNDEFINED] tests if a word is NOT defined (returns true if undefined)
\ Takes a word name as input: [UNDEFINED] <name>
\ FIND returns ( c-addr -- c-addr 0 ) if not found, or ( c-addr -- xt 1|-1 ) if found
\ NIP drops the address/xt, 0= inverts the flag (0 becomes -1 for undefined)
: [UNDEFINED] BL WORD FIND NIP 0= ; IMMEDIATE

\ Comments for documentation
( Stack: a b c -- b c a )
\ ROT is primitive

( Stack: a -- a a | a if non-zero )
\ ?DUP duplicates only if non-zero

( Stack: n lo hi -- flag )
\ WITHIN tests if lo <= n < hi
\ Example: 5 3 10 WITHIN gives true (non-zero)

\ Floating point test compatibility
\ SET-NEAR and SET-EXACT control FP comparison tolerance in test suites
\ Our FP implementation uses System.Double with inherent precision
\ These are stubs for test suite compatibility
: SET-NEAR ; \ Enable approximate FP equality (no-op for us)
: SET-EXACT ; \ Enable exact FP equality (no-op for us)


