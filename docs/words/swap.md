# SWAP

## NAME

`SWAP`

## SYNOPSIS

`SWAP ( a b -- b a )`

## DESCRIPTION

Swap top two items ( a b -- b a )

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
RND-TEST-BLOCK DUP BLOCK SWAP BLOCK = -> TRUE
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
- [`BUFFER`](buffer.md)
- [`DUP`](dup.md)
