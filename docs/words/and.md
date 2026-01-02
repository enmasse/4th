# AND

## NAME

`AND`

## SYNOPSIS

`AND ( a b -- a&b )`

## DESCRIPTION

Bitwise AND of two numbers ( a b -- a&b )

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
0 0 AND -> 0
```

Source: `tests/forth-tests/core.fr`

```forth
0 1 AND -> 0
```

Source: `tests/forth-tests/core.fr`

```forth
1 0 AND -> 0
```

Source: `tests/forth-tests/core.fr`

## SEE ALSO

- (none yet)
