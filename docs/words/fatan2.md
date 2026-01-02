# FATAN2

## NAME

`FATAN2` — arc tangent of y/x

## SYNOPSIS

`FATAN2 ( f: y x -- radians )`

## DESCRIPTION

FATAN2 ( f: y x -- radians ) - arc tangent of y/x

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
0E 1E fatan2 0E f= -> true
```

Source: `tests/forth-tests/fp/ak-fp-test.fth`

```forth
1E 1E fatan2 0.7854E tf= -> true
```

Source: `tests/forth-tests/fp/ak-fp-test.fth`

```forth
-1E 1E fatan2 -0.7854E tf= -> true
```

Source: `tests/forth-tests/fp/ak-fp-test.fth`

## SEE ALSO

- [`f=`](f_.md)
