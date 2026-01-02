# EMPTY-BUFFERS

## NAME

`EMPTY-BUFFERS`

## SYNOPSIS

`EMPTY-BUFFERS ( -- )`

## DESCRIPTION

EMPTY-BUFFERS ( -- ) unassign all block buffers

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
EMPTY-BUFFERS ->
```

Source: `tests/forth-tests/blocktest.fth`

```forth
BLK @ EMPTY-BUFFERS BLK @ = -> TRUE
```

Source: `tests/forth-tests/blocktest.fth`

```forth
SCR @ EMPTY-BUFFERS SCR @ = -> TRUE
```

Source: `tests/forth-tests/blocktest.fth`

## SEE ALSO

- [`=`](_.md)
- [`@`](_.md)
- [`BLK`](blk.md)
- [`SCR`](scr.md)
