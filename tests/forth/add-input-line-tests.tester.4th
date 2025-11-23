INCLUDE "../ttester.4th"
\ ADD-INPUT-LINE tests using tester harness
TESTING ADD-INPUT-LINE tests

T{ 
  S" HELLO" ADD-INPUT-LINE
  CREATE B 16 ALLOT
  B 10 READ-LINE DROP
  B C@ B 1 + C@ B 2 + C@ B 3 + C@ B 4 + C@
-> 72 69 76 76 79
}T

T{
  CREATE C 16 ALLOT
  5 C !
  72 C 1 + C! 69 C 2 + C! 76 C 3 + C! 76 C 4 + C! 79 C 5 + C!
  C ADD-INPUT-LINE
  CREATE B 16 ALLOT
  B 10 READ-LINE DROP
  B C@ B 1 + C@ B 2 + C@ B 3 + C@ B 4 + C@
-> 72 69 76 76 79
}T

T{
  CREATE D 16 ALLOT
  72 D C! 69 D 1 + C! 76 D 2 + C! 76 D 3 + C! 79 D 4 + C!
  D 5 ADD-INPUT-LINE
  CREATE B 16 ALLOT
  B 10 READ-LINE DROP
  B C@ B 1 + C@ B 2 + C@ B 3 + C@ B 4 + C@
-> 72 69 76 76 79
}T
