\ Test floating-point literals with E notation (no explicit exponent)

\ Test simple E notation
1.0E F. CR   \ Should print 1.0
2.0E F. CR   \ Should print 2.0
3.14E F. CR  \ Should print 3.14
-1.0E F. CR  \ Should print -1.0

\ Test simple e notation (lowercase)
0e F. CR     \ Should print 0.0
5e F. CR     \ Should print 5.0

\ Test arithmetic with these literals
1.0E 2.0E F+ F. CR  \ Should print 3.0
3.14E 2.0E F* F. CR \ Should print 6.28

\ Success message
CR .( All floating-point E notation tests passed! ) CR
