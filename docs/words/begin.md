# BEGIN

## NAME

`BEGIN`

## SYNOPSIS

`BEGIN`

## DESCRIPTION

Begin a loop construct

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
: GI4 BEGIN DUP 1+ DUP 5 > UNTIL ; ->
```

Source: `tests/forth-tests/core.fr`

```forth
: GI5 BEGIN DUP 2 >
WHILE DUP 5 < WHILE DUP 1+ REPEAT 123 ELSE 345 THEN ; ->
```

Source: `tests/forth-tests/core.fr`

## SEE ALSO

- [`1+`](1_.md)
- [`:`](_.md)
- [`;`](_.md)
- [`<`](_.md)
- [`>`](_.md)
- [`DUP`](dup.md)
- [`ELSE`](else.md)
- [`REPEAT`](repeat.md)
- [`THEN`](then.md)
- [`UNTIL`](until.md)
- [`WHILE`](while.md)
