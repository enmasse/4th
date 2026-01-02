# FSIN

## NAME

`FSIN` — floating-point sine

## SYNOPSIS

`FSIN ( r -- r )`

## DESCRIPTION

FSIN ( r -- r ) - floating-point sine

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
0E fsin 0E f= -> true
```

Source: `tests/forth-tests/fp/ak-fp-test.fth`

```forth
pi fsin 0E tf= -> true
```

Source: `tests/forth-tests/fp/ak-fp-test.fth`

```forth
pi2/ fsin 1E tf= -> true
```

Source: `tests/forth-tests/fp/ak-fp-test.fth`

## SEE ALSO

- [`f=`](f_.md)
