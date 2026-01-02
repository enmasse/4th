# FASIN

## NAME

`FASIN` — floating-point arcsine

## SYNOPSIS

`FASIN ( r -- r )`

## DESCRIPTION

FASIN ( r -- r ) - floating-point arcsine

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
0E fasin 0E f= -> true
```

Source: `tests/forth-tests/fp/ak-fp-test.fth`

```forth
0.5E fasin pi f/ 0.1667E tf= -> true
```

Source: `tests/forth-tests/fp/ak-fp-test.fth`

```forth
1E fasin pi f/ 0.5E tf= -> true
```

Source: `tests/forth-tests/fp/ak-fp-test.fth`

## SEE ALSO

- [`f/`](f_.md)
- [`f=`](f_.md)
