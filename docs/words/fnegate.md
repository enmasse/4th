# FNEGATE

## NAME

`FNEGATE`

## SYNOPSIS

`FNEGATE ( f -- -f )`

## DESCRIPTION

Negate floating ( f -- -f )

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
0. d>f fnegate f>d -> 0.
```

Source: `tests/forth-tests/fp/ak-fp-test.fth`

```forth
7. d>f fnegate f>d -> -7.
```

Source: `tests/forth-tests/fp/ak-fp-test.fth`

```forth
-3. d>f fnegate f>d -> 3.
```

Source: `tests/forth-tests/fp/ak-fp-test.fth`

## SEE ALSO

- [`d>f`](d_f.md)
- [`f>d`](f_d.md)
