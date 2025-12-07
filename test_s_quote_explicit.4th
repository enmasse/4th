\ Test S" with explicit stack checks

s" HELLO"

\ We should have 2 items: address and length
\ Let's try to use them

\ Duplicate the length
DUP .  \ Should print 5
CR

\ Drop length, leaving just address
DROP

\ Try to fetch a character from the address
C@ .  \ Should print 72 ('H')
CR

BYE
