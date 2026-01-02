# I

## NAME

`I`

## SYNOPSIS

`I`

## DESCRIPTION

Push current loop index

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
: GD1 DO I LOOP ; ->
```

Source: `tests/forth-tests/core.fr`

```forth
: GD2 DO I -1 +LOOP ; ->
```

Source: `tests/forth-tests/core.fr`

```forth
: GD5 123 SWAP 0 DO I 4 > IF DROP 234 LEAVE THEN LOOP ; ->
```

Source: `tests/forth-tests/core.fr`

## SEE ALSO

- [`+LOOP`](_loop.md)
- [`:`](_.md)
- [`;`](_.md)
- [`>`](_.md)
- [`DO`](do.md)
- [`DROP`](drop.md)
- [`IF`](if.md)
- [`LEAVE`](leave.md)
- [`LOOP`](loop.md)
- [`SWAP`](swap.md)
- [`THEN`](then.md)
