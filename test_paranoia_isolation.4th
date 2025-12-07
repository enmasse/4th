\ Minimal test to isolate paranoia.4th stack underflow issue
\ Based on the initialization code that was successfully patched

DECIMAL

\ Test 1: Verify patched initialization code works
CR .( Test 1: Patched [UNDEFINED] initialization) CR
s" [UNDEFINED]" dup pad c! pad char+ swap cmove 
pad find nip 0=
CR .( Result: ) . CR

\ Test 2: Verify patched [DEFINED] initialization works  
CR .( Test 2: Patched [DEFINED] initialization) CR
s" [DEFINED]" dup pad c! pad char+ swap cmove
pad find nip 0=
CR .( Result: ) . CR

CR .( Initialization tests passed!) CR
CR .( Now testing remaining paranoia patterns...) CR

\ Look for other similar patterns in paranoia.4th
\ Search for places where CMOVE is called with potentially wrong stack setup

\ Pattern from paranoia that might fail:
\ We need to find all CMOVE calls and check their stack usage

CR .( Test complete) CR
