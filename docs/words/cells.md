# CELLS

## NAME

`CELLS` — multiply by cell size

## SYNOPSIS

`CELLS ( n -- n*cellsize )`

## DESCRIPTION

CELLS ( n -- n*cellsize ) - multiply by cell size

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
\ ( -- ) record the pre-test depth.
DEPTH START-DEPTH ! 0 XCURSOR ! F{ ;
: -> \ ( ... -- ) record depth and contents of stack.
DEPTH DUP ACTUAL-DEPTH ! \ record depth
START-DEPTH @ > IF \ if there is something on the stack
DEPTH START-DEPTH @ - 0 DO \ save them
ACTUAL-RESULTS I CELLS + !
LOOP
THEN
F-> ;
:
```

Source: `tests/tester.fs`

```forth
\ ( -- ) syntactic sugar.
DEPTH START-DEPTH ! 0 XCURSOR ! F{ ;
: ->		\ ( ... -- ) record depth and contents of stack.
DEPTH DUP ACTUAL-DEPTH !		\ record depth
START-DEPTH @ > IF		\ if there is something on the stack
DEPTH START-DEPTH @ - 0 DO ACTUAL-RESULTS I CELLS + ! LOOP \ save them
THEN
F-> ;
:
```

Source: `tests/ttester.4th`

```forth
1ST 1 CELLS + -> 2ND
```

Source: `tests/forth-tests/core.fr`

## SEE ALSO

- [`!`](_.md)
- [`+`](_.md)
- [`-`](_.md)
- [`:`](_.md)
- [`;`](_.md)
- [`>`](_.md)
- [`@`](_.md)
- [`DEPTH`](depth.md)
- [`DO`](do.md)
- [`DUP`](dup.md)
- [`I`](i.md)
- [`IF`](if.md)
