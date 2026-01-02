# UM/MOD

## NAME

`UM/MOD` — unsigned division of double-cell by single-cell

## SYNOPSIS

`UM/MOD ( ud u -- urem uquot )`

## DESCRIPTION

UM/MOD ( ud u -- urem uquot ) - unsigned division of double-cell by single-cell

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
0 0 1 UM/MOD -> 0 0
```

Source: `tests/forth-tests/core.fr`

```forth
1 0 1 UM/MOD -> 0 1
```

Source: `tests/forth-tests/core.fr`

```forth
1 0 2 UM/MOD -> 1 0
```

Source: `tests/forth-tests/core.fr`

## SEE ALSO

- (none yet)
