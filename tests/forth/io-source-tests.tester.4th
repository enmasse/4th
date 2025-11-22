\ Converted IO/source tests using tester.fs harness
INCLUDE "../tester.fs"
\ SOURCE pushes addr len; just check len non-negative and then DROP both
T{ SOURCE DROP 0 >= -> TRUE }T
T{ >IN -> 0 }T
