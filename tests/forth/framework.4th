INCLUDE "../tester.fs" \ load tester harness first
\ Compatibility wrappers mapping older minimal harness to ANS tester
: ASSERT-TRUE ( flag -- ) 0= IF S" ASSERT-TRUE failed" ERROR THEN ;
: ASSERT= ( a b -- ) 2dup = 0= IF S" ASSERT= failed" ERROR ELSE 2drop THEN ;
: RUN-TEST ( xt -- ) SPAWN JOIN ;
: TEST-CASE ( "name" -- ) TYPE CR ' RUN-TEST ;
: RUN-TESTS ( -- ) ;
