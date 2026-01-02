# FMAX

## NAME

`FMAX` — return the maximum of r1 and r2

## SYNOPSIS

`FMAX ( r1 r2 -- r3 )`

## DESCRIPTION

FMAX ( r1 r2 -- r3 ) - return the maximum of r1 and r2

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
-2. d>f 1. d>f fmax f>d -> 1.
```

Source: `tests/forth-tests/fp/ak-fp-test.fth`

```forth
-1. d>f -2. d>f fmax f>d -> -1.
```

Source: `tests/forth-tests/fp/ak-fp-test.fth`

```forth
-1. d>f 0. d>f fmax f>d -> 0.
```

Source: `tests/forth-tests/fp/ak-fp-test.fth`

## SEE ALSO

- [`d>f`](d_f.md)
- [`f>d`](f_d.md)
