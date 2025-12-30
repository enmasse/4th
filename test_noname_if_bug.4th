\ Minimal test for :noname inside [IF] issue

\ This should work
:noname 42 ; execute .

\ This should also work
1 [IF]
  :noname 99 ; execute .
[THEN]

\ This is the pattern from fatan2-test.fs that fails
1 [IF]
:noname 100 ; execute
[THEN]

\ Try to use a word that doesn't exist
\ This should give "Undefined word: normal" in INTERPRET mode
\ But if we're stuck in compile mode, it will say "in definition"
normal

.( If you see this, the test passed! ) CR
