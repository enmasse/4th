INCLUDE "../ttester.4th"
\ Converted control flow tests using tester.fs harness
TESTING Control flow
: TEST-IF 1 IF 2 ELSE 3 THEN ;
: TEST-LOOP 0 3 0 DO I + LOOP ;
T{ TEST-IF TEST-LOOP -> 2 3 }T
