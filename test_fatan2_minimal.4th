\ Minimal reproduction of the fatan2-test issue

\ Load ttester
s" tests/ttester.4th" INCLUDED

\ Set verbose to true like fatan2-test does
true verbose !

\ Check verbose value
verbose @ . CR

\ This is the pattern from lines 86-92 of fatan2-test.fs
verbose @ [IF]
:noname  ( -- fp.separate? )
  depth >r 1e depth >r fdrop 2r> = ; execute
cr .( floating-point and data stacks )
[IF] .( *separate*) [ELSE] .( *not separate*) [THEN]
cr
[THEN]

\ This should work now
testing normal values

.( Test complete ) CR
