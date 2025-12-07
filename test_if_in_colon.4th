\ Test IF inside colon definition
\ This should work without error

: TEST-IF
  1 IF
    ." Inside IF" CR
  THEN
;

TEST-IF
." Test passed!" CR
