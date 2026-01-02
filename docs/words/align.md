# ALIGN

## NAME

`ALIGN` — align dictionary pointer to cell boundary

## SYNOPSIS

`ALIGN ( -- )`

## DESCRIPTION

ALIGN ( -- ) - align dictionary pointer to cell boundary

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
ALIGN UNUSED UNUSED0 ! 0 , UNUSED CELL+ UNUSED0 @ = -> TRUE
```

Source: `tests/forth-tests/coreexttest.fth`

```forth
HERE 1 ALLOT ALIGN HERE SWAP - ALMNT = -> <TRUE>
```

Source: `tests/forth2012-test-suite/src/core.fr`

## SEE ALSO

- [`!`](_.md)
- [`,`](_.md)
- [`-`](_.md)
- [`=`](_.md)
- [`@`](_.md)
- [`ALLOT`](allot.md)
- [`CELL+`](cell_.md)
- [`HERE`](here.md)
- [`SWAP`](swap.md)
- [`UNUSED`](unused.md)
