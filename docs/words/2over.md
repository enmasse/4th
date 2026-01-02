# 2OVER

## NAME

`2OVER` — copy pair two down into top

## SYNOPSIS

`2OVER ( a b c d -- a b c d a b )`

## DESCRIPTION

2OVER ( a b c d -- a b c d a b ) - copy pair two down into top

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
1 2 3 4 2OVER -> 1 2 3 4 1 2
```

Source: `tests/forth-tests/core.fr`

```forth
1 2 3 4 2OVER -> 1 2 3 4 1 2
```

Source: `tests/forth2012-test-suite-local/src/core.fr`

```forth
1 2 3 4 2OVER -> 1 2 3 4 1 2
```

Source: `tests/forth2012-test-suite/src/core.fr`

## SEE ALSO

- (none yet)
