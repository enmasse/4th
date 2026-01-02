# FLOOR

## NAME

`FLOOR` — round r1 to integral value not greater than r1 (ANS Forth returns float)

## SYNOPSIS

`FLOOR ( r1 -- r2 )`

## DESCRIPTION

FLOOR ( r1 -- r2 ) - round r1 to integral value not greater than r1 (ANS Forth returns float)

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
2E floor 2E f= -> true
```

Source: `tests/forth-tests/fp/ak-fp-test.fth`

```forth
1.5E floor 1E f= -> true
```

Source: `tests/forth-tests/fp/ak-fp-test.fth`

```forth
-0.5E floor -1E f= -> true
```

Source: `tests/forth-tests/fp/ak-fp-test.fth`

## SEE ALSO

- [`f=`](f_.md)
