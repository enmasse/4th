\ Test to verify _bracketIfSkipping behavior
\ This should print only "Line 1", "Line 2", and "Line 6"

.( Line 1 before conditional ) CR
-1 [IF]
.( Line 2 in TRUE branch ) CR
[ELSE]
.( Line 4 in FALSE branch - should NOT print ) CR
[THEN]
.( Line 6 after conditional ) CR
