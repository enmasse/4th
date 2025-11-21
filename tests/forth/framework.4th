\ Forth test harness - minimal
: ASSERT-TRUE ( flag -- ) 0= IF S" ASSERT-TRUE failed" ABORT THEN ;
: ASSERT= ( a b -- ) 2dup = 0= IF S" ASSERT= failed" ABORT ELSE 2drop THEN ;
: RUN-TEST ( xt -- ) SPAWN JOIN ;
: TEST-CASE ( "name" -- ) TYPE CR ' RUN-TEST ;
: RUN-TESTS ( -- ) ;
