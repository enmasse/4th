\ Test S" stack behavior

CR .( Testing S" stack behavior ) CR
CR .( Initial stack: ) .S CR

\ Test S"
s" HELLO"
CR .( After S" HELLO": ) .S CR

\ Check stack depth - should be 2
DEPTH 2 = IF
  CR .( CORRECT: Stack has 2 items ) CR
ELSE
  CR .( ERROR: Stack should have 2 items but has ) DEPTH . CR
THEN

\ Top should be length (5)
DUP 5 = IF
  CR .( CORRECT: Length is 5 ) CR
ELSE
  CR .( ERROR: Length should be 5 but is ) DUP . CR
THEN
DROP

\ Next should be address
DUP 0 > IF
  CR .( CORRECT: Address is positive: ) DUP . CR
ELSE
  CR .( ERROR: Address should be positive: ) DUP . CR
THEN

\ Try to fetch first character from address
DUP C@ 72 = IF
  CR .( CORRECT: First char is 'H' (72) ) CR
ELSE
  CR .( ERROR: First char should be 'H' (72) but is ) DUP C@ . CR
THEN

DROP DROP

CR .( Test complete ) CR
