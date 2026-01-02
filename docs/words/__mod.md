# */MOD

## NAME

`*/MOD`

## SYNOPSIS

`*/MOD ( n1 n2 d -- rem quot )`

## DESCRIPTION

Multiply n1*n2 then divide by d returning remainder and quotient ( n1 n2 d -- rem quot )

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
0 2 1 */MOD -> 0 2 1 T*/MOD
```

Source: `tests/forth-tests/core.fr`

```forth
1 2 1 */MOD -> 1 2 1 T*/MOD
```

Source: `tests/forth-tests/core.fr`

```forth
2 2 1 */MOD -> 2 2 1 T*/MOD
```

Source: `tests/forth-tests/core.fr`

## SEE ALSO

- (none yet)
