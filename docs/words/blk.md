# BLK

## NAME

`BLK` — push current block number

## SYNOPSIS

`BLK ( -- n )`

## DESCRIPTION

BLK ( -- n ) - push current block number

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
BLK DUP ALIGNED = -> TRUE
```

Source: `tests/forth-tests/blocktest.fth`

```forth
BLK @ RND-TEST-BLOCK BUFFER DROP BLK @ = -> TRUE
```

Source: `tests/forth-tests/blocktest.fth`

```forth
BLK @ RND-TEST-BLOCK BLOCK  DROP BLK @ = -> TRUE
```

Source: `tests/forth-tests/blocktest.fth`

## SEE ALSO

- [`=`](_.md)
- [`@`](_.md)
- [`BLOCK`](block.md)
- [`BUFFER`](buffer.md)
- [`DROP`](drop.md)
- [`DUP`](dup.md)
