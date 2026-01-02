# PAD

## NAME

`PAD` — push address of scratch pad buffer

## SYNOPSIS

`PAD ( -- addr )`

## DESCRIPTION

PAD ( -- addr ) - push address of scratch pad buffer

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
PAD DROP ->
```

Source: `tests/forth-tests/coreexttest.fth`

```forth
0 INVERT PAD C! ->
```

Source: `tests/forth-tests/coreexttest.fth`

```forth
PAD C@ CONSTANT MAXCHAR ->
```

Source: `tests/forth-tests/coreexttest.fth`

## SEE ALSO

- [`C!`](c_.md)
- [`C@`](c_.md)
- [`CONSTANT`](constant.md)
- [`DROP`](drop.md)
- [`INVERT`](invert.md)
