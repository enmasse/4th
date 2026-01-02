# OVER

## NAME

`OVER`

## SYNOPSIS

`OVER ( a b -- a b a )`

## DESCRIPTION

Copy second item to top ( a b -- a b a )

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
RND-TEST-BLOCK                      \ blk
0 OVER PREPARE-RND-BLOCK            \ blk hash
UPDATE FLUSH                        \ blk hash
OVER 0 SWAP PREPARE-RND-BLOCK DROP  \ blk hash
FLUSH ( with no preliminary UPDATE) \ blk hash
SWAP BLOCK 1024 ELF-HASH = -> TRUE
```

Source: `tests/forth-tests/blocktest.fth`

```forth
SCR @ RND-TEST-BLOCK DUP TLS6 LOAD                SCR @ OVER 2= -> TRUE
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
- [`@`](_.md)
- [`BLOCK`](block.md)
- [`CHAR`](char.md)
- [`DROP`](drop.md)
- [`DUP`](dup.md)
- [`EMPTY-BUFFERS`](empty_buffers.md)
- [`FLUSH`](flush.md)
- [`LOAD`](load.md)
- [`SCR`](scr.md)
- [`SWAP`](swap.md)
- [`UPDATE`](update.md)
