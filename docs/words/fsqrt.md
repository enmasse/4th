# FSQRT

## NAME

`FSQRT` — floating-point square root

## SYNOPSIS

`FSQRT ( r -- r )`

## DESCRIPTION

FSQRT ( r -- r ) - floating-point square root

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
4E fsqrt 2E tf= -> true
```

Source: `tests/forth-tests/fp/ak-fp-test.fth`

```forth
2E fsqrt 1.4142E tf= -> true
```

Source: `tests/forth-tests/fp/ak-fp-test.fth`

```forth
0E fsqrt 0E f= -> true
```

Source: `tests/forth-tests/fp/ak-fp-test.fth`

## SEE ALSO

- [`f=`](f_.md)
