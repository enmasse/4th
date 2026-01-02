# D>S

## NAME

`D>S` — convert double-cell to single-cell by taking the high cell

## SYNOPSIS

`D>S ( d -- n )`

## DESCRIPTION

D>S ( d -- n ) - convert double-cell to single-cell by taking the high cell

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
1234  0 D>S ->  1234
```

Source: `tests/forth-tests/doubletest.fth`

```forth
-1234 -1 D>S -> -1234
```

Source: `tests/forth-tests/doubletest.fth`

```forth
MAX-INTD  0 D>S -> MAX-INTD
```

Source: `tests/forth-tests/doubletest.fth`

## SEE ALSO

- (none yet)
