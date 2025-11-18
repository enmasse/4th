\ Example: write a file then read it back using core file primitives
\ Demonstrates WRITE-FILE, APPEND-FILE, READ-FILE, FILE-EXISTS

: WRITE-TEST  ( -- ) \ writes sample.txt
  S" Hello file" "sample.txt" WRITE-FILE ;

: APPEND-TEST  ( -- ) \ append a line to sample.txt
  S" \nAppended line" "sample.txt" APPEND-FILE ;

: READ-TEST  ( -- ) \ read and print sample.txt if it exists
  "sample.txt" FILE-EXISTS IF
    "sample.txt" READ-FILE TYPE CR
  ELSE
    S" File not found" TYPE CR
  THEN ;

\ Usage examples (interpret in the REPL):
\ WRITE-TEST
\ APPEND-TEST
\ READ-TEST
