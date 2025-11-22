\ Converted memory test using tester.fs harness
INCLUDE "framework.4th"
CREATE BUF 16 ALLOT
: TEST-STORE ( -- ) T{ 123 BUF ! BUF @ -> 123 }T ;
S" STORE" TEST-CASE TEST-STORE
