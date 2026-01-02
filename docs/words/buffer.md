# BUFFER

## NAME

`BUFFER`

## SYNOPSIS

`BUFFER ( u -- a-addr )`

## DESCRIPTION

BUFFER ( u -- a-addr ) assign a block buffer to block u

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
RND-TEST-BLOCK BUFFER DUP ALIGNED = -> TRUE
```

Source: `tests/forth-tests/blocktest.fth`

```forth
RND-TEST-BLOCK DUP BUFFER SWAP BUFFER = -> TRUE
```

Source: `tests/forth-tests/blocktest.fth`

```forth
RND-TEST-BLOCK DUP BLOCK SWAP BUFFER = -> TRUE
```

Source: `tests/forth-tests/blocktest.fth`

## SEE ALSO

- [`=`](_.md)
- [`BLOCK`](block.md)
- [`DUP`](dup.md)
- [`SWAP`](swap.md)
