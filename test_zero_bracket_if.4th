\ Test 0 [IF] ... [THEN] pattern (used for multi-line comments)
\ This pattern appears in paranoia.4th line 93

CR .( Testing 0 [IF] comment block pattern...) CR

0 [IF]
  This entire block should be skipped
  Including any IF statements that appear here
  IF this were executed, it would cause an error
  THEN
  : SOME-WORD that should never be defined ;
[THEN]

CR .( Pattern worked - block was properly skipped!) CR

\ Now test with actual colon definitions after the comment
0 [IF]
  : BAD-WORD ." This should never print" ;
  IF we executed this it would fail THEN
[THEN]

: GOOD-WORD ." This should work" CR ;
GOOD-WORD

CR .( All tests passed!) CR
