# DMIN

## NAME

`DMIN` — return the minimum of two double-cell numbers

## SYNOPSIS

`DMIN ( d1 d2 -- d3 )`

## DESCRIPTION

DMIN ( d1 d2 -- d3 ) - return the minimum of two double-cell numbers

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
1.  2. DMIN ->  1.
```

Source: `tests/forth-tests/doubletest.fth`

```forth
1.  0. DMIN ->  0.
```

Source: `tests/forth-tests/doubletest.fth`

```forth
1. -1. DMIN -> -1.
```

Source: `tests/forth-tests/doubletest.fth`

## SEE ALSO

- (none yet)
