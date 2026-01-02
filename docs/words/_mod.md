# /MOD

## NAME

`/MOD`

## SYNOPSIS

`/MOD ( a b -- rem quot )`

## DESCRIPTION

Divide and return remainder and quotient ( a b -- rem quot )

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
0 1 /MOD -> 0 1 T/MOD
```

Source: `tests/forth-tests/core.fr`

```forth
1 1 /MOD -> 1 1 T/MOD
```

Source: `tests/forth-tests/core.fr`

```forth
2 1 /MOD -> 2 1 T/MOD
```

Source: `tests/forth-tests/core.fr`

## SEE ALSO

- (none yet)
