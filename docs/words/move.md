# MOVE

## NAME

`MOVE` — copy u bytes from src to dst

## SYNOPSIS

`MOVE ( src dst u -- )`

## DESCRIPTION

MOVE ( src dst u -- ) - copy u bytes from src to dst

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
FBUF FBUF 3 CHARS MOVE ->
```

Source: `tests/forth-tests/core.fr`

```forth
SBUF FBUF 0 CHARS MOVE ->
```

Source: `tests/forth-tests/core.fr`

```forth
SBUF FBUF 1 CHARS MOVE ->
```

Source: `tests/forth-tests/core.fr`

## SEE ALSO

- [`CHARS`](chars.md)
