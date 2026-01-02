# MAX

## NAME

`MAX`

## SYNOPSIS

`MAX ( a b -- max )`

## DESCRIPTION

Return larger of two numbers ( a b -- max )

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
0 1 MAX -> 1
```

Source: `tests/forth-tests/core.fr`

```forth
1 2 MAX -> 2
```

Source: `tests/forth-tests/core.fr`

```forth
-1 0 MAX -> 0
```

Source: `tests/forth-tests/core.fr`

## SEE ALSO

- (none yet)
