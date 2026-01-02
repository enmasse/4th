# DABS

## NAME

`DABS` — absolute value of double-cell

## SYNOPSIS

`DABS ( d -- |d| )`

## DESCRIPTION

DABS ( d -- |d| ) - absolute value of double-cell

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
1. DABS -> 1.
```

Source: `tests/forth-tests/doubletest.fth`

```forth
-1. DABS -> 1.
```

Source: `tests/forth-tests/doubletest.fth`

```forth
MAX-2INT DABS -> MAX-2INT
```

Source: `tests/forth-tests/doubletest.fth`

## SEE ALSO

- (none yet)
