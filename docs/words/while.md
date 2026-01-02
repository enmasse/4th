# WHILE

## NAME

`WHILE`

## SYNOPSIS

`WHILE`

## DESCRIPTION

Begin a conditional part of a BEGIN...REPEAT loop

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
: LT26 {: A :} 0 BEGIN A WHILE 2 + A 1- TO A REPEAT ; ->
```

Source: `tests/forth-tests/localstest.fth`

## SEE ALSO

- [`+`](_.md)
- [`1+`](1_.md)
- [`1-`](1_.md)
- [`:`](_.md)
- [`;`](_.md)
- [`<`](_.md)
- [`>`](_.md)
- [`BEGIN`](begin.md)
- [`DUP`](dup.md)
- [`ELSE`](else.md)
- [`REPEAT`](repeat.md)
- [`THEN`](then.md)
