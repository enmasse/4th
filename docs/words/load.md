# LOAD

## NAME

`LOAD`

## SYNOPSIS

`LOAD ( n -- )`

## DESCRIPTION

LOAD ( n -- ) load and interpret block n

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
RND-TEST-BLOCK DUP BLANK-BUFFER DROP UPDATE FLUSH LOAD ->
```

Source: `tests/forth-tests/blocktest.fth`

```forth
BLK @ RND-TEST-BLOCK DUP BLANK-BUFFER DROP UPDATE FLUSH LOAD BLK @ = -> TRUE
```

Source: `tests/forth-tests/blocktest.fth`

```forth
BLOCK-RND RND-TEST-BLOCK 2DUP TL1 LOAD = -> TRUE
```

Source: `tests/forth-tests/blocktest.fth`

## SEE ALSO

- [`2DUP`](2dup.md)
- [`=`](_.md)
- [`@`](_.md)
- [`BLK`](blk.md)
- [`DROP`](drop.md)
- [`DUP`](dup.md)
- [`FLUSH`](flush.md)
- [`UPDATE`](update.md)
