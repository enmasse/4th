# LSHIFT

## NAME

`LSHIFT`

## SYNOPSIS

`LSHIFT ( a n -- (a<<n)`

## DESCRIPTION

Left shift a by n bits ( a n -- (a<<n) )

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
1 0 LSHIFT -> 1
```

Source: `tests/forth-tests/core.fr`

```forth
1 1 LSHIFT -> 2
```

Source: `tests/forth-tests/core.fr`

```forth
1 2 LSHIFT -> 4
```

Source: `tests/forth-tests/core.fr`

## SEE ALSO

- (none yet)
