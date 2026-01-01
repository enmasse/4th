\ Test nested [IF] with stack inspection
variable verbose
-1 verbose !

verbose @ [IF]
:noname  ( -- fp.separate? )
  depth >r 1e depth >r fdrop 2r> = ; execute
.( After execute, stack: ) .S CR
cr .( floating-point and data stacks )
[IF] .( *separate*) [ELSE] .( *not separate*) [THEN]
cr
[THEN]

.( Test complete) CR
