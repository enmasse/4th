\ Converted COUNT test using tester.fs harness
INCLUDE "framework.4th"
: TEST-COUNT ( -- ) T{ S" hello" COUNT SWAP DROP -> 5 }T ;
S" COUNT" TEST-CASE TEST-COUNT
