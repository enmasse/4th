# U<

## NAME

`U<`

## SYNOPSIS

`U< (unsigned)`

## DESCRIPTION

Unsigned compare: push -1 if second < top (unsigned) else 0 ( u1 u2 -- flag )

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
0 1 U< -> <TRUE>
```

Source: `tests/forth-tests/core.fr`

```forth
1 2 U< -> <TRUE>
```

Source: `tests/forth-tests/core.fr`

```forth
0 MID-UINT U< -> <TRUE>
```

Source: `tests/forth-tests/core.fr`

## SEE ALSO

- (none yet)
