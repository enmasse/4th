\ Converted arithmetic tests using tester.fs T{ -> }T harness
INCLUDE "framework.4th"
: TEST-ADD ( -- ) T{ 2 3 + -> 5 }T ;
: TEST-MUL ( -- ) T{ 6 7 * -> 42 }T ;
: TEST-DIV ( -- ) T{ 20 4 / -> 5 }T ;
\ Register tests using existing TEST-CASE wrapper
S" ADD" TEST-CASE TEST-ADD
S" MUL" TEST-CASE TEST-MUL
S" DIV" TEST-CASE TEST-DIV
