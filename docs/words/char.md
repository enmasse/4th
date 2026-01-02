# CHAR

## NAME

`CHAR` — push character code for character literal

## SYNOPSIS

`CHAR`

## DESCRIPTION

CHAR <c> - push character code for character literal

## FLAGS

- Module: `(core)`
- Immediate: `True`
- Async: `False`

## EXAMPLES

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

```forth
RND-TEST-BLOCK                  \ blk
DUP BLANK-BUFFER                \ blk blk-addr1
CHAR \ OVER 1023 CHARS + C!     \ blk blk-addr1
1024 ELF-HASH                   \ blk hash
UPDATE FLUSH                    \ blk hash
SWAP BLOCK                      \ hash blk-addr2
1024 ELF-HASH = -> TRUE
```

Source: `tests/forth-tests/blocktest.fth`

```forth
RND-TEST-BLOCK                    \ blk
DUP BLANK-BUFFER                  \ blk blk-addr1
1024 ELF-HASH                     \ blk hash
UPDATE FLUSH                      \ blk hash
OVER BLOCK CHAR \ SWAP C!         \ blk hash
UPDATE EMPTY-BUFFERS FLUSH        \ blk hash
SWAP BLOCK                        \ hash blk-addr2
1024 ELF-HASH = -> TRUE
```

Source: `tests/forth-tests/blocktest.fth`

## SEE ALSO

- [`=`](_.md)
- [`BLOCK`](block.md)
- [`DUP`](dup.md)
- [`EMPTY-BUFFERS`](empty_buffers.md)
- [`FLUSH`](flush.md)
- [`OVER`](over.md)
- [`SWAP`](swap.md)
- [`UPDATE`](update.md)
