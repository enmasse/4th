\ Example Forth tests using harness
INCLUDE "framework.4th"  \ load helpers
: SAMPLE-TEST ( -- )
  1 1 = ASSERT-TRUE
  S" sample test passed" TYPE CR ;

S" SAMPLE-TEST" TEST-CASE SAMPLE-TEST
