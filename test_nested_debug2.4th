\ Test nested [IF] with debug output

\ First define [DEFINED] if needed
[UNDEFINED] [DEFINED] [IF]
: [DEFINED]  [UNDEFINED] 0= ; IMMEDIATE
[THEN]

.( About to test TESTWORD ) CR
[UNDEFINED] TESTWORD . CR  \ Should print -1 (true)

.( Entering outer [IF] ) CR
[UNDEFINED] TESTWORD [IF]
  .( Inside outer [IF], checking DUP ) CR
  [DEFINED] DUP . CR  \ Should print -1 (true)
  
  .( About to enter inner [IF] ) CR
  [DEFINED] DUP [IF]  
    .( Inside inner [IF], defining TESTWORD ) CR
    : TESTWORD DUP ;
    .( TESTWORD defined ) CR
  [ELSE] 
    .( In inner [ELSE] - should not see this! ) CR
  [THEN]
  .( Exited inner [IF] ) CR
[THEN]
.( Exited outer [IF] ) CR

\ Test if TESTWORD exists
[UNDEFINED] TESTWORD . CR  \ Should print 0 (false) if defined

\ Test it
.( Testing TESTWORD ) CR
5 TESTWORD . .
