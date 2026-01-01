\ Test S" stack behavior without IF

CR .( Testing S" stack behavior ) CR
CR .( Initial stack: ) .S CR

\ Test S"
s" HELLO"
CR .( After S" HELLO": ) .S CR

CR .( Stack depth: ) DEPTH . CR

\ Test if we have 2 items
DEPTH 2 = 
CR .( Depth check result: ) . CR

CR .( Test complete ) CR
BYE
