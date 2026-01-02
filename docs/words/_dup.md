# ?DUP

## NAME

`?DUP`

## SYNOPSIS

`?DUP ( n -- n n | 0 )`

## DESCRIPTION

?DUP ( n -- n n | 0 ) duplicate if non-zero

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
0 ?DUP -> 0
```

Source: `tests/forth-tests/core.fr`

```forth
1 ?DUP -> 1 1
```

Source: `tests/forth-tests/core.fr`

```forth
-1 ?DUP -> -1 -1
```

Source: `tests/forth-tests/core.fr`

## SEE ALSO

- (none yet)
