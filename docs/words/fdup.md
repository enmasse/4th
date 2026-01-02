# FDUP

## NAME

`FDUP` — duplicate top floating item

## SYNOPSIS

`FDUP ( r -- r r )`

## DESCRIPTION

FDUP ( r -- r r ) - duplicate top floating item

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
-7. d>f fdup f>d f>d -> -7. -7.
```

Source: `tests/forth-tests/fp/ak-fp-test.fth`

```forth
1. d>f fdup f+ f>d -> 2.
```

Source: `tests/forth-tests/fp/ak-fp-test.fth`

```forth
1. d>f fdup f- f>d -> 0.
```

Source: `tests/forth-tests/fp/ak-fp-test.fth`

## SEE ALSO

- [`d>f`](d_f.md)
- [`f+`](f_.md)
- [`f-`](f_.md)
- [`f>d`](f_d.md)
