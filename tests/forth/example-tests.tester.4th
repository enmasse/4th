\ Converted sample test using tester.fs harness
INCLUDE "framework.4th"
: SAMPLE-TEST ( -- ) T{ 1 1 + -> 2 }T ;
S" SAMPLE-TEST" TEST-CASE SAMPLE-TEST
