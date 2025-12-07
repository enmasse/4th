\ Test file to reproduce the issue
\ This simulates paranoia.4th structure

\ Line with [IF] that should skip everything until [THEN]
0 [IF]
    \ Inside this block, there might be IF statements
    \ that are part of explanatory comments or code blocks
    : TEST-WORD 1 IF 2 THEN ;
    Some text here with IF in it
[THEN]

\ This should execute
." Test passed" CR
