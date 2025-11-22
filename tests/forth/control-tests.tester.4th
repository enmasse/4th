\ Converted control flow tests using tester.fs harness
INCLUDE "framework.4th"
T{ 1 IF 2 ELSE 3 THEN -> 2 }T
T{ 0 3 0 DO I + LOOP -> 3 }T
