\ Test the exact pattern from paranoia.4th initialization

\ Show stack before
CR .( Stack before S" test: ) .S CR

\ Test the original buggy pattern
s" [UNDEFINED]" 
CR .( After S": ) .S CR

dup 
CR .( After DUP: ) .S CR

pad 
CR .( After PAD: ) .S CR

c! 
CR .( After C!: ) .S CR

pad 
CR .( After PAD again: ) .S CR

char+ 
CR .( After CHAR+: ) .S CR

swap 
CR .( After SWAP: ) .S CR

CR .( About to call CMOVE with: ) .S CR

\ This should fail with stack underflow
\ cmove

CR .( Test complete ) CR
