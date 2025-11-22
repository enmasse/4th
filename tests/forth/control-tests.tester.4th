\ Converted control flow tests using tester.fs harness
INCLUDE "framework.4th"
: TEST-IF ( -- ) T{ 1 IF 2 ELSE 3 THEN -> 2 }T ;
: TEST-LOOP ( -- ) T{ 0 3 0 DO I + LOOP -> 3 }T ;
S" IF" TEST-CASE TEST-IF
S" LOOP" TEST-CASE TEST-LOOP
