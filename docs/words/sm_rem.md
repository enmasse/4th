# SM/REM

## NAME

`SM/REM` — floored division of double-cell by single-cell

## SYNOPSIS

`SM/REM ( d n -- rem quot )`

## DESCRIPTION

SM/REM ( d n -- rem quot ) - floored division of double-cell by single-cell

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
0 S>D 1 SM/REM -> 0 0
```

Source: `tests/forth-tests/core.fr`

```forth
1 S>D 1 SM/REM -> 0 1
```

Source: `tests/forth-tests/core.fr`

```forth
2 S>D 1 SM/REM -> 0 2
```

Source: `tests/forth-tests/core.fr`

## SEE ALSO

- [`S>D`](s_d.md)
