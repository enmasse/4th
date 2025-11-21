\ Isolated COUNT test with diagnostics
INCLUDE "framework.4th"
: TEST-COUNT S" hello" COUNT SWAP DROP 5 = DUP ASSERT-TRUE ;
S" COUNT" TEST-CASE TEST-COUNT
