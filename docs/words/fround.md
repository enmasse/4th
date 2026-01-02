# FROUND

## NAME

`FROUND` — convert floating-point number to integer by rounding

## SYNOPSIS

`FROUND ( r -- n )`

## DESCRIPTION

FROUND ( r -- n ) - convert floating-point number to integer by rounding

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
2E fround 2E f= -> true
```

Source: `tests/forth-tests/fp/ak-fp-test.fth`

```forth
1.5E fround 2E f= -> true
```

Source: `tests/forth-tests/fp/ak-fp-test.fth`

```forth
1.4999E fround 1E f= -> true
```

Source: `tests/forth-tests/fp/ak-fp-test.fth`

## SEE ALSO

- [`f=`](f_.md)
