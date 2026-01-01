\ Minimal test to trace paranoia.4th failure pattern
\ This recreates the exact pattern that causes desynchronization

DECIMAL

\ Define [undefined] if not already defined
: [undefined] ( "name" -- flag )
  BL WORD FIND NIP 0=
; IMMEDIATE

\ Test 1: Single [undefined] check with [IF] in interpret mode
.( Test 1: Single [undefined] check) CR
[undefined] NONEXISTENT-WORD-1 [IF]
  .( This should print) CR
[THEN]

\ Test 2: [undefined] in false [IF] branch
.( Test 2: [undefined] in false [IF] branch) CR
0 [IF]
  [undefined] NONEXISTENT-WORD-2 [IF]
    .( This should NOT print) CR
  [THEN]
[THEN]

\ Test 3: Multiple [undefined] checks in false [IF] branch
.( Test 3: Multiple [undefined] in false [IF]) CR
0 [IF]
  [undefined] WORD1 [IF] : WORD1 123 ; [THEN]
  [undefined] WORD2 [IF] : WORD2 456 ; [THEN]
  [undefined] WORD3 [IF] : WORD3 789 ; [THEN]
[THEN]

\ Test 4: Pattern from paranoia.4th - conditional definition
.( Test 4: Conditional definition pattern) CR
[undefined] TESTWORD [IF]
  : TESTWORD ( -- n ) 42 ;
[THEN]

\ Test 5: The critical pattern - [undefined] INSIDE a false [IF]
\ with a colon definition that has regular IF
.( Test 5: Critical pattern) CR
0 [IF]
  [undefined] CRITWORD [IF]
    : CRITWORD ( n -- )
      DUP 0> IF
        .( Positive) CR
      ELSE
        .( Non-positive) CR
      THEN
    ;
  [THEN]
[THEN]

.( All tests completed successfully!) CR
