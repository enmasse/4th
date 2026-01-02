# FROT

## NAME

`FROT` — rotate top three floating items

## SYNOPSIS

`FROT ( r1 r2 r3 -- r2 r3 r1 )`

## DESCRIPTION

FROT ( r1 r2 r3 -- r2 r3 r1 ) - rotate top three floating items

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
1. 2. 3. d>f d>f d>f frot f>d f>d f>d -> 3. 1. 2.
```

Source: `tests/forth-tests/fp/ak-fp-test.fth`

```forth
1. 2. 3. d>f d>f d>f frot f>d f>d f>d -> 3. 1. 2.
```

Source: `tests/forth2012-test-suite-local/src/fp/ak-fp-test.fth`

```forth
1. 2. 3. d>f d>f d>f frot f>d f>d f>d -> 3. 1. 2.
```

Source: `tests/forth2012-test-suite/src/fp/ak-fp-test.fth`

## SEE ALSO

- [`d>f`](d_f.md)
- [`f>d`](f_d.md)
