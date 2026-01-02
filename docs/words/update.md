# UPDATE

## NAME

`UPDATE` — mark the current block as updated

## SYNOPSIS

`UPDATE ( -- )`

## DESCRIPTION

UPDATE ( -- ) - mark the current block as updated

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
- [`DROP`](drop.md)
- [`DUP`](dup.md)
- [`FLUSH`](flush.md)
- [`SWAP`](swap.md)
