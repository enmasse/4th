# FEXP

## NAME

`FEXP` — floating-point exponential (e^r)

## SYNOPSIS

`FEXP ( r -- r )`

## DESCRIPTION

FEXP ( r -- r ) - floating-point exponential (e^r)

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
0E fexp 1E f= -> true
```

Source: `tests/forth-tests/fp/ak-fp-test.fth`

```forth
1E fexp 2.7183E tf= -> true
```

Source: `tests/forth-tests/fp/ak-fp-test.fth`

```forth
-1E fexp 0.3679E tf= -> true
```

Source: `tests/forth-tests/fp/ak-fp-test.fth`

## SEE ALSO

- [`f=`](f_.md)
