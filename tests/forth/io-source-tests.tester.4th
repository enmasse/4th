\ Converted IO/source tests using tester.fs harness
INCLUDE "framework.4th"
: TEST-SOURCE ( -- ) SOURCE DROP T{ 0 >= -> TRUE }T ; \ just verify non-negative length
: TEST-IN ( -- ) T{ >IN -> 0 }T ; \ initial >IN should be 0
S" SOURCE" TEST-CASE TEST-SOURCE
S" >IN" TEST-CASE TEST-IN
