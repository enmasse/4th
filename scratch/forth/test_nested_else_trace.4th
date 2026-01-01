\ Test nested [IF] with [ELSE] to trace execution
\ This should define TEST-FLAG with value -1

.( Before outer IF ) CR

-1 [IF]
  .( Inside outer IF - TRUE branch ) CR
  -1 [IF]
    .( Inside inner IF - TRUE branch ) CR
    -1
  [ELSE]
    .( Inside inner ELSE - should NOT see this! ) CR
    0
  [THEN]
  .( After inner THEN ) CR
[ELSE]
  .( Inside outer ELSE - should NOT see this! ) CR
  0
[THEN]

.( After outer THEN, about to define CONSTANT ) CR
CONSTANT TEST-FLAG

.( Defined TEST-FLAG ) CR
TEST-FLAG . CR
