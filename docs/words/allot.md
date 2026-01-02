# ALLOT

## NAME

`ALLOT` — reserve u cells in dictionary

## SYNOPSIS

`ALLOT ( u -- )`

## DESCRIPTION

ALLOT ( u -- ) - reserve u cells in dictionary

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
HERE 5 ALLOT -5 ALLOT HERE = -> <TRUE>
```

Source: `tests/forth-tests/coreplustest.fth`

```forth
HERE 0 ALLOT HERE = -> <TRUE>
```

Source: `tests/forth-tests/coreplustest.fth`

## SEE ALSO

- [`-`](_.md)
- [`=`](_.md)
- [`ALIGN`](align.md)
- [`HERE`](here.md)
- [`SWAP`](swap.md)
