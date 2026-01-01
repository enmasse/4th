\ Simple test for [UNDEFINED]
\ First, test with a word that doesn't exist
." Testing [UNDEFINED] with nonexistent word: " CR
[UNDEFINED] FOOBARNONEXISTENT .S CR

\ Define a word
: TESTWORD 42 ;

\ Test with a word that now exists
." Testing [UNDEFINED] with defined word TESTWORD: " CR
[UNDEFINED] TESTWORD .S CR

\ Test in a conditional
." Testing conditional definition: " CR
[UNDEFINED] NEWWORD [IF]
  ." Defining NEWWORD..." CR
  : NEWWORD 99 ;
[THEN]

NEWWORD . CR

\ Test that redefinition is skipped
." Testing skip redefinition: " CR
: EXISTINGWORD 100 ;
[UNDEFINED] EXISTINGWORD [IF]
  ." This should not print!" CR
  : EXISTINGWORD 200 ;
[THEN]

EXISTINGWORD . CR
