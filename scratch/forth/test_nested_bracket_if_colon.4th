\ Test nested [IF] with colon definition inside
\ This mimics the structure in paranoia.4th lines 412-416

\ First define [DEFINED] if needed
[UNDEFINED] [DEFINED] [IF]
: [DEFINED]  [UNDEFINED] 0= ; IMMEDIATE
[THEN]

\ Now the problematic nested structure
[UNDEFINED] TESTWORD [IF]
  [DEFINED] DUP [IF]  
    : TESTWORD DUP ;
  [ELSE] 
    ." Should not see this" CR
  [THEN]
[THEN]

\ Test it
5 TESTWORD . .
