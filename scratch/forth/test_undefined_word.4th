: [undefined] bl word find nip 0= ; immediate
[undefined] TESTWORD [IF] ." TESTWORD is undefined" CR [THEN]
: TESTWORD ;
[undefined] TESTWORD [IF] ." TESTWORD is STILL undefined?!" CR [THEN]
." Done" CR
BYE
