\ Simple test for [ELSE] skipping on subsequent lines
\ This should print only the first message, not the second

-1 [IF]
  .( First message - should print ) CR
[ELSE]
  .( Second message - should NOT print ) CR
[THEN]
.( Done ) CR
