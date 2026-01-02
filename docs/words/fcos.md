# FCOS

## NAME

`FCOS` — floating-point cosine

## SYNOPSIS

`FCOS ( r -- r )`

## DESCRIPTION

FCOS ( r -- r ) - floating-point cosine

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
0E fcos 1E f= -> true
```

Source: `tests/forth-tests/fp/ak-fp-test.fth`

```forth
pi fcos 1E fnegate tf= -> true
```

Source: `tests/forth-tests/fp/ak-fp-test.fth`

```forth
pi2/ fcos 0E tf= -> true
```

Source: `tests/forth-tests/fp/ak-fp-test.fth`

## SEE ALSO

- [`f=`](f_.md)
- [`fnegate`](fnegate.md)
