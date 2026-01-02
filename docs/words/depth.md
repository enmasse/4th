# DEPTH

## NAME

`DEPTH`

## SYNOPSIS

`DEPTH ( -- n )`

## DESCRIPTION

Return current stack depth ( -- n )

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
DEPTH -> 0
```

Source: `tests/forth-tests/core.fr`

```forth
0 DEPTH -> 0 1
```

Source: `tests/forth-tests/core.fr`

```forth
0 1 DEPTH -> 0 1 2
```

Source: `tests/forth-tests/core.fr`

## SEE ALSO

- (none yet)
