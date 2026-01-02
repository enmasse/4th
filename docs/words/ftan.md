# FTAN

## NAME

`FTAN` — floating-point tangent

## SYNOPSIS

`FTAN ( r -- r )`

## DESCRIPTION

FTAN ( r -- r ) - floating-point tangent

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
0E ftan 0E f= -> true
```

Source: `tests/forth-tests/fp/ak-fp-test.fth`

```forth
pi ftan 0E tf= -> true
```

Source: `tests/forth-tests/fp/ak-fp-test.fth`

```forth
pi4/ ftan 1E tf= -> true
```

Source: `tests/forth-tests/fp/ak-fp-test.fth`

## SEE ALSO

- [`f=`](f_.md)
