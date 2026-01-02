# UM*

## NAME

`UM*` — unsigned multiply to double-cell (low then high)

## SYNOPSIS

`UM* ( u1 u2 -- ud )`

## DESCRIPTION

UM* ( u1 u2 -- ud ) - unsigned multiply to double-cell (low then high)

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
0 0 UM* -> 0 0
```

Source: `tests/forth-tests/core.fr`

```forth
0 1 UM* -> 0 0
```

Source: `tests/forth-tests/core.fr`

```forth
1 0 UM* -> 0 0
```

Source: `tests/forth-tests/core.fr`

## SEE ALSO

- (none yet)
