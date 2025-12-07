\ Simple test for [ELSE] after TRUE [IF]

.( Before [IF] ) CR
-1 [IF]
  .( Inside [IF] TRUE branch ) CR
[ELSE]
  .( Inside [ELSE] - should NOT see this! ) CR
[THEN]
.( After [THEN] ) CR
