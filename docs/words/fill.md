# FILL

## NAME

`FILL` — fill u bytes at addr with ch

## SYNOPSIS

`FILL ( addr u ch -- )`

## DESCRIPTION

FILL ( addr u ch -- ) - fill u bytes at addr with ch

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
FBUF 0 20 FILL ->
```

Source: `tests/forth-tests/core.fr`

```forth
FBUF 1 20 FILL ->
```

Source: `tests/forth-tests/core.fr`

```forth
FBUF 3 20 FILL ->
```

Source: `tests/forth-tests/core.fr`

## SEE ALSO

- (none yet)
