\ Converted control flow tests using tester.fs harness
INCLUDE "../tester.fs"
: TEST-IF 1 IF 2 ELSE 3 THEN ;
T{ TEST-IF -> 2 }T
: TEST-LOOP 0 3 0 DO I + LOOP ;
T{ TEST-LOOP -> 3 }T
