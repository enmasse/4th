# SAVE-BUFFERS

## NAME

`SAVE-BUFFERS` — transfer the contents of each updated block buffer to mass storage

## SYNOPSIS

`SAVE-BUFFERS ( -- )`

## DESCRIPTION

SAVE-BUFFERS ( -- ) - transfer the contents of each updated block buffer to mass storage

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
LIMIT-TEST-BLOCK FIRST-TEST-BLOCK WRITE-RND-BLOCKS-WITH-HASH SAVE-BUFFERS
LIMIT-TEST-BLOCK FIRST-TEST-BLOCK READ-BLOCKS-AND-HASH = -> TRUE
```

Source: `tests/forth-tests/blocktest.fth`

```forth
RND-TEST-BLOCK DUP BLANK-BUFFER
SAVE-BUFFERS        SWAP BUFFER = -> TRUE
```

Source: `tests/forth-tests/blocktest.fth`

```forth
RND-TEST-BLOCK DUP BLANK-BUFFER
UPDATE SAVE-BUFFERS SWAP BUFFER = -> TRUE
```

Source: `tests/forth-tests/blocktest.fth`

## SEE ALSO

- [`=`](_.md)
- [`BUFFER`](buffer.md)
- [`DUP`](dup.md)
- [`SWAP`](swap.md)
- [`UPDATE`](update.md)
