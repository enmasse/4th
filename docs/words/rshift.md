# RSHIFT

## NAME

`RSHIFT`

## SYNOPSIS

`RSHIFT ( a n -- (a>>n)`

## DESCRIPTION

Logical right shift a by n bits ( a n -- (a>>n) )

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
1 0 RSHIFT -> 1
```

Source: `tests/forth-tests/core.fr`

```forth
1 1 RSHIFT -> 0
```

Source: `tests/forth-tests/core.fr`

```forth
2 1 RSHIFT -> 1
```

Source: `tests/forth-tests/core.fr`

## SEE ALSO

- (none yet)
