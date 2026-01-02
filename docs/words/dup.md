# DUP

## NAME

`DUP`

## SYNOPSIS

`DUP ( x -- x x )`

## DESCRIPTION

Duplicate top stack item ( x -- x x )

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
RND-TEST-BLOCK BLOCK DUP ALIGNED = -> TRUE
```

Source: `tests/forth-tests/blocktest.fth`

## SEE ALSO

- [`!`](_.md)
- [`+`](_.md)
- [`-`](_.md)
- [`:`](_.md)
- [`;`](_.md)
- [`=`](_.md)
- [`>`](_.md)
- [`@`](_.md)
- [`BLOCK`](block.md)
- [`CELLS`](cells.md)
- [`DEPTH`](depth.md)
- [`DO`](do.md)
