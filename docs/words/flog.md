# FLOG

## NAME

`FLOG` — floating-point natural logarithm

## SYNOPSIS

`FLOG ( r -- r )`

## DESCRIPTION

FLOG ( r -- r ) - floating-point natural logarithm

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
1E flog 0E f= -> true
```

Source: `tests/forth-tests/fp/ak-fp-test.fth`

```forth
0.1E flog -1E tf= -> true
```

Source: `tests/forth-tests/fp/ak-fp-test.fth`

```forth
10E flog 1E tf= -> true
```

Source: `tests/forth-tests/fp/ak-fp-test.fth`

## SEE ALSO

- [`f=`](f_.md)
