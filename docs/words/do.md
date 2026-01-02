# DO

## NAME

`DO`

## SYNOPSIS

`DO ( limit start -- )`

## DESCRIPTION

Begin a counted loop ( limit start -- )

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
: GD2 DO I -1 +LOOP ; ->
```

Source: `tests/forth-tests/core.fr`

```forth
: GD3 DO 1 0 DO J LOOP LOOP ; ->
```

Source: `tests/forth-tests/core.fr`

## SEE ALSO

- [`+LOOP`](_loop.md)
- [`:`](_.md)
- [`;`](_.md)
- [`I`](i.md)
- [`J`](j.md)
- [`LOOP`](loop.md)
