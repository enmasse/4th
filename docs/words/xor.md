# XOR

## NAME

`XOR`

## SYNOPSIS

`XOR ( a b -- a^b )`

## DESCRIPTION

Bitwise XOR of two numbers ( a b -- a^b )

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
0S 0S XOR -> 0S
```

Source: `tests/forth-tests/core.fr`

```forth
0S 1S XOR -> 1S
```

Source: `tests/forth-tests/core.fr`

```forth
1S 0S XOR -> 1S
```

Source: `tests/forth-tests/core.fr`

## SEE ALSO

- (none yet)
