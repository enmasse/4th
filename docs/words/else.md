# ELSE

## NAME

`ELSE`

## SYNOPSIS

`ELSE`

## DESCRIPTION

Begin else-part of an if construct

## FLAGS

- Module: `(core)`
- Immediate: `True`
- Async: `False`

## EXAMPLES

```forth
: BITSSET? IF 0 0 ELSE 0 THEN ; ->
```

Source: `tests/forth-tests/core.fr`

```forth
: GI2 IF 123 ELSE 234 THEN ; ->
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
- [`BEGIN`](begin.md)
- [`DUP`](dup.md)
- [`IF`](if.md)
- [`REPEAT`](repeat.md)
- [`THEN`](then.md)
- [`WHILE`](while.md)
