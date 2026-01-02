# D=

## NAME

`D=` — true if d1 == d2

## SYNOPSIS

`D= ( d1 d2 -- flag )`

## DESCRIPTION

D= ( d1 d2 -- flag ) - true if d1 == d2

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
-1. -1. D= -> TRUE
```

Source: `tests/forth-tests/doubletest.fth`

```forth
-1.  0. D= -> FALSE
```

Source: `tests/forth-tests/doubletest.fth`

```forth
-1.  1. D= -> FALSE
```

Source: `tests/forth-tests/doubletest.fth`

## SEE ALSO

- (none yet)
