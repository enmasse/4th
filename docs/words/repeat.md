# REPEAT

## NAME

`REPEAT`

## SYNOPSIS

`REPEAT`

## DESCRIPTION

End a BEGIN...REPEAT loop

## FLAGS

- Module: `(core)`
- Immediate: `True`
- Async: `False`

## EXAMPLES

```forth
: GI3 BEGIN DUP 5 < WHILE DUP 1+ REPEAT ; ->
```

Source: `tests/forth-tests/core.fr`

```forth
: GI5 BEGIN DUP 2 >
WHILE DUP 5 < WHILE DUP 1+ REPEAT 123 ELSE 345 THEN ; ->
```

Source: `tests/forth-tests/core.fr`

```forth
: UNS1 DUP 0 > IF 9 SWAP BEGIN 1+ DUP 3 > IF EXIT THEN REPEAT ; ->
```

Source: `tests/forth-tests/coreplustest.fth`

## SEE ALSO

- [`1+`](1_.md)
- [`:`](_.md)
- [`;`](_.md)
- [`<`](_.md)
- [`>`](_.md)
- [`BEGIN`](begin.md)
- [`DUP`](dup.md)
- [`ELSE`](else.md)
- [`EXIT`](exit.md)
- [`IF`](if.md)
- [`SWAP`](swap.md)
- [`THEN`](then.md)
