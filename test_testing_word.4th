\ Test the TESTING word pattern from ttester

VARIABLE VERBOSE
TRUE VERBOSE !

: TESTING	\ ( -- ) TALKING COMMENT.
   SOURCE VERBOSE @
   IF DUP >R TYPE CR R> >IN !
   ELSE >IN ! DROP
   THEN ;

\ This should print "normal values" and skip the rest
testing normal values

\ If we get here, the test passed
CR .( Test passed - normal values was skipped ) CR
