# FM/MOD

## NAME

`FM/MOD`

## SYNOPSIS

`FM/MOD ( n1 n2 -- rem quot )`

## DESCRIPTION

Floored division ( n1 n2 -- rem quot ) with remainder having sign of divisor

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
0 S>D 1 FM/MOD -> 0 0
```

Source: `tests/forth-tests/core.fr`

```forth
1 S>D 1 FM/MOD -> 0 1
```

Source: `tests/forth-tests/core.fr`

```forth
2 S>D 1 FM/MOD -> 0 2
```

Source: `tests/forth-tests/core.fr`

## SEE ALSO

- [`S>D`](s_d.md)
