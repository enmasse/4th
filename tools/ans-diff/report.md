ANS-diff report
===============

Generated: 2025-11-20

Summary
-------
- Found primitives in code: 149
- ANS core words present: 81
- ANS core words missing: 18
- Other primitives in code (not in ANS list): 68

Primitives found in code (149)
------------------------------
!, #, #>, #S, ', *, */, +, +!, ,, -, -ROT, ., .S, .\, /, /MOD, 0<>, 0=, 2>R, 2DUP, 2OVER, 2R>, 2SWAP, :, ;, <, <#, <=, <>, =, >, >=, >NUMBER, >R, @, ABORT, ALLOT, AND, APPEND-FILE, AWAIT, BASE, BEGIN, BIND, BYE, C!, C@, CATCH, CHAR, CONSTANT, COUNT, CR, CREATE, DECIMAL, DEFER, DEPTH, DO, DOES>, DROP, DUMP, DUP, ELSE, EMIT, END-MODULE, ERASE, EXECUTE, EXIT, F!, F*, F+, F-, F., F/, F@, FCONSTANT, FILE-EXISTS, FILL, FNEGATE, FORGET, FUTURE, FVARIABLE, HELP, HERE, HEX, HOLD, I, IF, IMMEDIATE, INCLUDE, INVERT, IS, JOIN, LATEST, LEAVE, LITERAL, LOAD-ASM, LOAD-ASM-TYPE, LOOP, LSHIFT, MARKER, MAX, MIN, MOD, MODULE, MOVE, NEGATE, NIP, NOT, OR, OVER, PICK, POSTPONE, QUIT, R>, R@, READ-FILE, RECURSE, REPEAT, ROT, RP@, RSHIFT, S, SEE, SET-ORDER, SIGN, SPACE, SPACES, SPAWN, STATE, SWAP, S\, TASK, TASK?, THEN, THROW, TO, TRUE, TUCK, TYPE, U., UNLOOP, UNTIL, USING, VALUE, VARIABLE, WHILE, WITHIN, WORDS, WRITE-FILE, XOR, YIELD, [ , ]

ANS core words present (81)
--------------------------
,, ;, :, !, ., .S, ', [, ], @, #, #>, #S, <#, >NUMBER, ABORT, ALLOT, APPEND-FILE, AWAIT, BASE, BEGIN, BYE, C!, C@, CATCH, CONSTANT, COUNT, CR, CREATE, DECIMAL, DEFER, DO, DOES>, ELSE, EMIT, ERASE, EXIT, FILE-EXISTS, FILL, FORGET, FUTURE, GET-ORDER, HERE, HEX, HOLD, I, IF, IMMEDIATE, INCLUDE, IS, JOIN, KEY, KEY?, LEAVE, LITERAL, LOAD, LOOP, MARKER, MOVE, POSTPONE, QUIT, READ-FILE, RECURSE, REPEAT, SET-ORDER, SIGN, SPAWN, STATE, TASK, TASK?, THEN, THROW, TO, TYPE, UNLOOP, UNTIL, VALUE, VARIABLE, WHILE, WORDS, WRITE-FILE

ANS core words missing (18)
-------------------------
*/MOD, >IN, BLK, BLOCK, CLOSE-FILE, D-, D+, DEFINITIONS, EXPECT, FILE-SIZE, FORTH, LOAD, M*, OPEN-FILE, REPOSITION-FILE, SAVE, SOURCE, WORDLIST

Other primitives in code not in ANS list (68)
--------------------------------------------
-, -ROT, .\, *, */, /, /MOD, +, +!, <, <=, <>, =, >, >=, >R, 0<>, 0=, 2>R, 2DUP, 2OVER, 2R>, 2SWAP, AND, BIND, CHAR, DEPTH, DROP, DUMP, DUP, END-MODULE, EXECUTE, F-, F!, F., F@, F*, F/, F+, FCONSTANT, FNEGATE, FVARIABLE, HELP, INVERT, LATEST, LOAD-ASM, LOAD-ASM-TYPE, LSHIFT, MAX, MIN, MOD, MODULE, NEGATE, OR, OVER, PICK, R@, R>, ROT, RP@, RSHIFT, S, S\, SEE, SWAP, USING, XOR, YIELD

Notes
-----
- The "ANS core" list included here is a practical subset used for comparison; it may not exactly match every ANS Forth specification wording.
- Missing items include wordlist/search-order management (`GET-ORDER`, `SET-ORDER`, `WORDLIST`, `DEFINITIONS`, `FORTH`) and interactive input (`KEY`, `KEY?`, `ACCEPT`, `EXPECT`, `>IN`, `SOURCE`), plus file stream operations.

Next steps
----------
- Commit the `tools/ans-diff` tool and this report, or adjust the ANS core wordlist then re-run.
- Optionally export report as JSON for CI consumption.

