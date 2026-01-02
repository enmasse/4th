# DMAX

## NAME

`DMAX` — return the maximum of two double-cell numbers

## SYNOPSIS

`DMAX ( d1 d2 -- d3 )`

## DESCRIPTION

DMAX ( d1 d2 -- d3 ) - return the maximum of two double-cell numbers

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
1.  2. DMAX -> 2.
```

Source: `tests/forth-tests/doubletest.fth`

```forth
1.  0. DMAX -> 1.
```

Source: `tests/forth-tests/doubletest.fth`

```forth
1. -1. DMAX -> 1.
```

Source: `tests/forth-tests/doubletest.fth`

## SEE ALSO

- (none yet)
