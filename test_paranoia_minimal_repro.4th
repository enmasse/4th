\ Minimal reproduction of paranoia issue
\ This tests the specific pattern that causes problems

0 [IF]
: BadCond ( n a u -- )
    rot dup
    CASE
      0 OF ." FAILURE " ENDOF
      1 OF ." SERIOUS " ENDOF
      2 OF ." DEFECT "  ENDOF
      3 OF ." FLAW "    ENDOF
    ENDCASE
    type
;

: TstCond ( n  Valid  a u -- )
    rot 0=  IF  BadCond ." ." cr  ELSE 2drop drop THEN
;
[THEN]

\ Should not execute anything above
." Test completed" CR
