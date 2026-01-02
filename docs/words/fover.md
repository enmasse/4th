# FOVER

## NAME

`FOVER` — copy second floating item to top

## SYNOPSIS

`FOVER ( r1 r2 -- r1 r2 r1 )`

## DESCRIPTION

FOVER ( r1 r2 -- r1 r2 r1 ) - copy second floating item to top

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
-4. d>f -2. d>f fover f>d f>d f>d -> -4. -2. -4.
```

Source: `tests/forth-tests/fp/ak-fp-test.fth`

```forth
-4. d>f -2. d>f fover f>d f>d f>d -> -4. -2. -4.
```

Source: `tests/forth2012-test-suite-local/src/fp/ak-fp-test.fth`

```forth
-4. d>f -2. d>f fover f>d f>d f>d -> -4. -2. -4.
```

Source: `tests/forth2012-test-suite/src/fp/ak-fp-test.fth`

## SEE ALSO

- [`d>f`](d_f.md)
- [`f>d`](f_d.md)
