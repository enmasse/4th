\ Isolated duplicate of STORE test to help diagnose failures
INCLUDE "framework.4th"
CREATE BUF 16 ALLOT
: TEST-STORE 123 BUF ! BUF @ 123 = ASSERT-TRUE ;
S" STORE-ISO" TEST-CASE TEST-STORE
