# CHARS

## NAME

`CHARS` — multiply by char size

## SYNOPSIS

`CHARS ( n -- n*charsize )`

## DESCRIPTION

CHARS ( n -- n*charsize ) - multiply by char size

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
1STC 1 CHARS + -> 2NDC
```

Source: `tests/forth-tests/core.fr`

```forth
1 CHARS 1 < -> <FALSE>
```

Source: `tests/forth-tests/core.fr`

```forth
1 CHARS 1 CELLS > -> <FALSE>
```

Source: `tests/forth-tests/core.fr`

## SEE ALSO

- [`+`](_.md)
- [`<`](_.md)
- [`>`](_.md)
- [`CELLS`](cells.md)
