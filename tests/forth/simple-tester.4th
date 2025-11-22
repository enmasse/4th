INCLUDE "framework.4th"
\ Very small test using the tester harness (T{ -> }T)
: SIMPLE-ADD ( -- ) T{ 2 3 + -> 5 }T ;
S" SIMPLE-ADD" TEST-CASE SIMPLE-ADD
