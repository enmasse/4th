# F~

## NAME

`F~`

## SYNOPSIS

`F~ ( r1 r2 r3 -- flag )`

## DESCRIPTION

Floating point proximity test ( r1 r2 r3 -- flag )

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
10. d>f 11. d>f 1. d>f f~ -> false
```

Source: `tests/forth-tests/fp/ak-fp-test.fth`

```forth
-10. d>f -11. d>f 2. d>f f~ -> true
```

Source: `tests/forth-tests/fp/ak-fp-test.fth`

```forth
1. d>f 2. d>f 1. d>f f~ -> false
```

Source: `tests/forth-tests/fp/ak-fp-test.fth`

## SEE ALSO

- [`d>f`](d_f.md)
