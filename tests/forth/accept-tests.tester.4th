INCLUDE "../ttester.4th"
\ ACCEPT tests using tester harness
TESTING ACCEPT tests

T{
  S" hello" ADD-INPUT-LINE
  CREATE B 16 ALLOT
  B 10 ACCEPT
-> 5
  B C@ B 1 + C@ B 2 + C@ B 3 + C@ B 4 + C@
-> 104 101 108 108 111
}T

T{
  S" hello\rworld" ADD-INPUT-LINE
  CREATE B 16 ALLOT
  B 10 ACCEPT
-> 5
  B C@ B 1 + C@ B 2 + C@ B 3 + C@ B 4 + C@
-> 104 101 108 108 111
}T

T{
  S" hello\nworld" ADD-INPUT-LINE
  CREATE B 16 ALLOT
  B 10 ACCEPT
-> 5
  B C@ B 1 + C@ B 2 + C@ B 3 + C@ B 4 + C@
-> 104 101 108 108 111
}T

T{
  S" hel\rlo" ADD-INPUT-LINE
  CREATE B 16 ALLOT
  B 10 ACCEPT
-> 3
  B C@ B 1 + C@ B 2 + C@
-> 104 101 108
}T

T{
  S" abcdefghij" ADD-INPUT-LINE
  CREATE B 16 ALLOT
  B 5 ACCEPT
-> 5
  B C@ B 1 + C@ B 2 + C@ B 3 + C@ B 4 + C@
-> 97 98 99 100 101
}T

T{
  S" a\r\nb" ADD-INPUT-LINE
  CREATE B 16 ALLOT
  B 10 ACCEPT
-> 1
  B C@
-> 97
}T