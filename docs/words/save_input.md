# SAVE-INPUT

## NAME

`SAVE-INPUT` — save current input source state

## SYNOPSIS

`SAVE-INPUT ( -- xn ... x1 n )`

## DESCRIPTION

SAVE-INPUT ( -- xn ... x1 n ) - save current input source state

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
11111 SAVE-INPUT
SIV @
[?IF]
\?   0 SIV !
\?   RESTORE-INPUT
\?   NEVEREXECUTED
\?   33333
[?ELSE]
\? TESTING the -[ELSE]- part is executed
\? 22222
[?THEN]
-> 11111 0 22222
```

Source: `tests/forth-tests/filetest.fth`

```forth
11111 SAVE-INPUT
SIV @
[?IF]
\?   0 SIV !
\?   RESTORE-INPUT
\?   NEVEREXECUTED
\?   33333
[?ELSE]
\? TESTING the -[ELSE]- part is executed
\? 22222
[?THEN]
-> 11111 0 22222
```

Source: `tests/forth2012-test-suite-local/src/filetest.fth`

```forth
11111 SAVE-INPUT
SIV @
[?IF]
\?   0 SIV !
\?   RESTORE-INPUT
\?   NEVEREXECUTED
\?   33333
[?ELSE]
\? TESTING the -[ELSE]- part is executed
\? 22222
[?THEN]
-> 11111 0 22222
```

Source: `tests/forth2012-test-suite/src/filetest.fth`

## SEE ALSO

- [`@`](_.md)
