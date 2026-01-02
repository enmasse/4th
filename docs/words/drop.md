# DROP

## NAME

`DROP`

## SYNOPSIS

`DROP ( x -- )`

## DESCRIPTION

Drop top stack item ( x -- )

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
RND-TEST-BLOCK BLOCK DROP UPDATE ->
```

Source: `tests/forth-tests/blocktest.fth`

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
' TUF2-1 2RND-TEST-BLOCKS TUF2       \ run test procedure
SWAP DROP SWAP DROP 2= -> TRUE
```

Source: `tests/forth-tests/blocktest.fth`

## SEE ALSO

- [`'`](_.md)
- [`=`](_.md)
- [`BLOCK`](block.md)
- [`FLUSH`](flush.md)
- [`OVER`](over.md)
- [`SWAP`](swap.md)
- [`UPDATE`](update.md)
