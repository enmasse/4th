# J

## NAME

`J` — loop index of outer DO loop

## SYNOPSIS

`J ( -- n )`

## DESCRIPTION

J ( -- n ) - loop index of outer DO loop

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
: GD3 DO 1 0 DO J LOOP LOOP ; ->
```

Source: `tests/forth-tests/core.fr`

```forth
: GD4 DO 1 0 DO J LOOP -1 +LOOP ; ->
```

Source: `tests/forth-tests/core.fr`

```forth
: GD6  ( PAT: T{0 0},{0 0}{1 0}{1 1},{0 0}{1 0}{1 1}{2 0}{2 1}{2 2} )
0 SWAP 0 DO
I 1+ 0 DO I J + 3 = IF I UNLOOP I UNLOOP EXIT THEN 1+ LOOP
LOOP ; ->
```

Source: `tests/forth-tests/core.fr`

## SEE ALSO

- [`+`](_.md)
- [`+LOOP`](_loop.md)
- [`1+`](1_.md)
- [`:`](_.md)
- [`;`](_.md)
- [`=`](_.md)
- [`DO`](do.md)
- [`EXIT`](exit.md)
- [`I`](i.md)
- [`IF`](if.md)
- [`LOOP`](loop.md)
- [`SWAP`](swap.md)
