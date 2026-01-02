# FLN

## NAME

`FLN` — floating-point natural logarithm (alias for FLOG)

## SYNOPSIS

`FLN ( r -- r )`

## DESCRIPTION

FLN ( r -- r ) - floating-point natural logarithm (alias for FLOG)

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
1E fln 0E f= -> true
```

Source: `tests/forth-tests/fp/ak-fp-test.fth`

```forth
2.7183E fln 1E tf= -> true
```

Source: `tests/forth-tests/fp/ak-fp-test.fth`

```forth
0.36788E fln -1E tf= -> true
```

Source: `tests/forth-tests/fp/ak-fp-test.fth`

## SEE ALSO

- [`f=`](f_.md)
