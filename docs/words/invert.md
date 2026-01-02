# INVERT

## NAME

`INVERT`

## SYNOPSIS

`INVERT ( a -- ~a )`

## DESCRIPTION

Bitwise NOT of top item ( a -- ~a )

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
0 INVERT 1 AND -> 1
```

Source: `tests/forth-tests/core.fr`

```forth
1 INVERT 1 AND -> 0
```

Source: `tests/forth-tests/core.fr`

```forth
0S INVERT -> 1S
```

Source: `tests/forth-tests/core.fr`

## SEE ALSO

- [`AND`](and.md)
