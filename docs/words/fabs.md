# FABS

## NAME

`FABS` — floating-point absolute value

## SYNOPSIS

`FABS ( r -- |r| )`

## DESCRIPTION

FABS ( r -- |r| ) - floating-point absolute value

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
1. d>f fabs f>d -> 1.
```

Source: `tests/forth-tests/fp/ak-fp-test.fth`

```forth
-2. d>f fabs f>d -> 2.
```

Source: `tests/forth-tests/fp/ak-fp-test.fth`

```forth
t{ +0 +0 f/ fabs -> +nan }t
t{ +0 -0 f/ fabs -> +nan }t
t{ -0 +0 f/ fabs -> +nan }t
```

Source: `tests/forth-tests/fp/ieee-arith-test.fs`

## SEE ALSO

- [`d>f`](d_f.md)
- [`f/`](f_.md)
- [`f>d`](f_d.md)
