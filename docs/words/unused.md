# UNUSED

## NAME

`UNUSED` — return the number of cells remaining in the data space

## SYNOPSIS

`UNUSED ( -- u )`

## DESCRIPTION

UNUSED ( -- u ) - return the number of cells remaining in the data space

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
UNUSED DROP ->
```

Source: `tests/forth-tests/coreexttest.fth`

```forth
ALIGN UNUSED UNUSED0 ! 0 , UNUSED CELL+ UNUSED0 @ = -> TRUE
```

Source: `tests/forth-tests/coreexttest.fth`

```forth
UNUSED UNUSED0 ! 0 C, UNUSED CHAR+ UNUSED0 @ =
-> TRUE
```

Source: `tests/forth-tests/coreexttest.fth`

## SEE ALSO

- [`!`](_.md)
- [`,`](_.md)
- [`=`](_.md)
- [`@`](_.md)
- [`ALIGN`](align.md)
- [`C,`](c_.md)
- [`CELL+`](cell_.md)
- [`CHAR+`](char_.md)
- [`DROP`](drop.md)
