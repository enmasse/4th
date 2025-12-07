\ Test nested [IF] in interpret mode like fatan2-test.fs
variable verbose
-1 verbose !

verbose @ [IF]
:noname  ( -- fp.separate? )
  depth >r 1e depth >r fdrop 2r> = ; execute
cr .( floating-point and data stacks )
[IF] .( *separate*) [ELSE] .( *not separate*) [THEN]
cr
[THEN]

.( Test complete) CR
