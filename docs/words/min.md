# MIN

## NAME

`MIN`

## SYNOPSIS

`MIN ( a b -- min )`

## DESCRIPTION

Return smaller of two numbers ( a b -- min )

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
0 1 MIN -> 0
```

Source: `tests/forth-tests/core.fr`

```forth
1 2 MIN -> 1
```

Source: `tests/forth-tests/core.fr`

```forth
-1 0 MIN -> -1
```

Source: `tests/forth-tests/core.fr`

## SEE ALSO

- (none yet)
