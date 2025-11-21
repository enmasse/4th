\ Forth test harness - minimal
: ASSERT-TRUE ( flag -- ) 0= IF S" ASSERT-TRUE failed" THROW THEN ;
: ASSERT= ( a b -- ) 2dup = 0= IF S" ASSERT= failed" THROW ELSE 2drop THEN ;
: TEST-CASE ( "name" -- ) \ print name
  TYPE CR ;
: RUN-TESTS ( -- ) \ placeholder, runner will use INCLUDE on test files
  ;
