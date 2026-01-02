# FMIN

## NAME

`FMIN` — return the minimum of r1 and r2

## SYNOPSIS

`FMIN ( r1 r2 -- r3 )`

## DESCRIPTION

FMIN ( r1 r2 -- r3 ) - return the minimum of r1 and r2

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
1. d>f 2. d>f fmin f>d -> 1.
```

Source: `tests/forth-tests/fp/ak-fp-test.fth`

```forth
-3. d>f -4. d>f fmin f>d -> -4.
```

Source: `tests/forth-tests/fp/ak-fp-test.fth`

```forth
1. d>f 0. d>f fmin f>d -> 0.
```

Source: `tests/forth-tests/fp/ak-fp-test.fth`

## SEE ALSO

- [`d>f`](d_f.md)
- [`f>d`](f_d.md)
