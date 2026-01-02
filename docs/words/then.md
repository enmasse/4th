# THEN

## NAME

`THEN`

## SYNOPSIS

`THEN`

## DESCRIPTION

End an if construct

## FLAGS

- Module: `(core)`
- Immediate: `True`
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
: BITSSET? IF 0 0 ELSE 0 THEN ; ->
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
- [`CELLS`](cells.md)
- [`DEPTH`](depth.md)
- [`DO`](do.md)
- [`DUP`](dup.md)
- [`ELSE`](else.md)
