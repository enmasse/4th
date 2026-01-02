# LEAVE

## NAME

`LEAVE`

## SYNOPSIS

`LEAVE`

## DESCRIPTION

Leave the nearest DO...LOOP

## FLAGS

- Module: `(core)`
- Immediate: `True`
- Async: `False`

## EXAMPLES

```forth
: GD5 123 SWAP 0 DO I 4 > IF DROP 234 LEAVE THEN LOOP ; ->
```

Source: `tests/forth-tests/core.fr`

```forth
: GD5 123 SWAP 0 DO I 4 > IF DROP 234 LEAVE THEN LOOP ; ->
```

Source: `tests/forth2012-test-suite/src/core.fr`

```forth
: GD5 123 SWAP 0 DO I 4 > IF DROP 234 LEAVE THEN LOOP ; ->
```

Source: `tests/forth2012-test-suite-local/src/core.fr`

## SEE ALSO

- [`:`](_.md)
- [`;`](_.md)
- [`>`](_.md)
- [`DO`](do.md)
- [`DROP`](drop.md)
- [`I`](i.md)
- [`IF`](if.md)
- [`LOOP`](loop.md)
- [`SWAP`](swap.md)
- [`THEN`](then.md)
