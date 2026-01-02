# +LOOP

## NAME

`+LOOP` — end DO loop with runtime step ( step from stack )

## SYNOPSIS

`+LOOP ( step from stack )`

## DESCRIPTION

+LOOP - end DO loop with runtime step ( step from stack )

## FLAGS

- Module: `(core)`
- Immediate: `True`
- Async: `False`

## EXAMPLES

```forth
: GD2 DO I -1 +LOOP ; ->
```

Source: `tests/forth-tests/core.fr`

```forth
: GD4 DO 1 0 DO J LOOP -1 +LOOP ; ->
```

Source: `tests/forth-tests/core.fr`

```forth
: GD8 BUMP ! DO 1+ BUMP @ +LOOP ; ->
```

Source: `tests/forth-tests/coreplustest.fth`

## SEE ALSO

- [`!`](_.md)
- [`1+`](1_.md)
- [`:`](_.md)
- [`;`](_.md)
- [`@`](_.md)
- [`DO`](do.md)
- [`I`](i.md)
- [`J`](j.md)
- [`LOOP`](loop.md)
