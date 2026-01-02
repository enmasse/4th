# CELLS

## NAME

`CELLS` — multiply by cell size

## SYNOPSIS

`CELLS ( n -- n*cellsize )`

## DESCRIPTION

CELLS ( n -- n*cellsize ) - multiply by cell size

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
1ST 1 CELLS + -> 2ND
```

Source: `tests/forth-tests/core.fr`

```forth
1 CHARS 1 CELLS > -> <FALSE>
```

Source: `tests/forth-tests/core.fr`

```forth
1 CELLS 1 < -> <FALSE>
```

Source: `tests/forth-tests/core.fr`

## SEE ALSO

- [`+`](_.md)
- [`<`](_.md)
- [`>`](_.md)
- [`CHARS`](chars.md)
