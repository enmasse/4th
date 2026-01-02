# HERE

## NAME

`HERE` — push current dictionary allocation pointer

## SYNOPSIS

`HERE ( -- addr )`

## DESCRIPTION

HERE ( -- addr ) - push current dictionary allocation pointer

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
HERE 1 ALLOT ALIGN HERE SWAP - ALMNT = -> <TRUE>
```

Source: `tests/forth-tests/core.fr`

```forth
CR1 -> HERE
```

Source: `tests/forth-tests/core.fr`

```forth
' CR1 >BODY -> HERE
```

Source: `tests/forth-tests/core.fr`

## SEE ALSO

- [`'`](_.md)
- [`-`](_.md)
- [`=`](_.md)
- [`>BODY`](_body.md)
- [`ALIGN`](align.md)
- [`ALLOT`](allot.md)
- [`SWAP`](swap.md)
