# D>F

## NAME

`D>F` — convert double-cell integer to floating-point number

## SYNOPSIS

`D>F ( d -- r )`

## DESCRIPTION

D>F ( d -- r ) - convert double-cell integer to floating-point number

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
0. d>f fdepth f>d -> 1 0.
```

Source: `tests/forth-tests/fp/ak-fp-test.fth`

```forth
0. d>f fdrop fdepth -> 0
```

Source: `tests/forth-tests/fp/ak-fp-test.fth`

```forth
0. d>f 0. d>f fdrop fdepth f>d -> 1 0.
```

Source: `tests/forth-tests/fp/ak-fp-test.fth`

## SEE ALSO

- [`f>d`](f_d.md)
- [`fdepth`](fdepth.md)
- [`fdrop`](fdrop.md)
