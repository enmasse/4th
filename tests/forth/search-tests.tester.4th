INCLUDE "../ttester.4th"
TESTING SEARCH
T{ S" hello world" 11 S" world" 5 SEARCH DROP DROP -> -1 }T
T{ S" hello world" 11 S" foo" 3 SEARCH DROP DROP -> 0 }T
T{ S" abc" 3 S" a" 1 SEARCH DROP DROP -> -1 }T
T{ S" abc" 3 S" d" 1 SEARCH DROP DROP -> 0 }T
T{ S" " 0 S" " 0 SEARCH DROP DROP -> -1 }T
T{ S" test" 4 S" " 0 SEARCH DROP DROP -> -1 }T