# LOOP

## NAME

`LOOP`

## SYNOPSIS

`LOOP`

## DESCRIPTION

End a counted DO...LOOP

## FLAGS

- Module: `(core)`
- Immediate: `True`
- Async: `False`

## EXAMPLES

```forth
: GD1 DO I LOOP ; ->
```

Source: `tests/forth-tests/core.fr`

```forth
: GD3 DO 1 0 DO J LOOP LOOP ; ->
```

Source: `tests/forth-tests/core.fr`

```forth
: GD4 DO 1 0 DO J LOOP -1 +LOOP ; ->
```

Source: `tests/forth-tests/core.fr`

## SEE ALSO

- [`+LOOP`](_loop.md)
- [`:`](_.md)
- [`;`](_.md)
- [`DO`](do.md)
- [`I`](i.md)
- [`J`](j.md)
