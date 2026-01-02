# FLUSH

## NAME

`FLUSH` — perform SAVE-BUFFERS then unassign all block buffers

## SYNOPSIS

`FLUSH ( -- )`

## DESCRIPTION

FLUSH ( -- ) - perform SAVE-BUFFERS then unassign all block buffers

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
FLUSH ->
```

Source: `tests/forth-tests/blocktest.fth`

```forth
RND-TEST-BLOCK                 \ blk
DUP BLANK-BUFFER               \ blk blk-addr1
1024 ELF-HASH                  \ blk hash
UPDATE FLUSH                   \ blk hash
SWAP BLOCK                     \ hash blk-addr2
1024 ELF-HASH = -> TRUE
```

Source: `tests/forth-tests/blocktest.fth`

```forth
RND-TEST-BLOCK                  \ blk
DUP BLANK-BUFFER                \ blk blk-addr1
CHAR \ OVER C!                  \ blk blk-addr1
1024 ELF-HASH                   \ blk hash
UPDATE FLUSH                    \ blk hash
SWAP BLOCK                      \ hash blk-addr2
1024 ELF-HASH = -> TRUE
```

Source: `tests/forth-tests/blocktest.fth`

## SEE ALSO

- [`=`](_.md)
- [`BLOCK`](block.md)
- [`CHAR`](char.md)
- [`DUP`](dup.md)
- [`SWAP`](swap.md)
- [`UPDATE`](update.md)
