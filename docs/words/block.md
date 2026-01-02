# BLOCK

## NAME

`BLOCK`

## SYNOPSIS

`BLOCK ( n -- c-addr u )`

## DESCRIPTION

BLOCK ( n -- c-addr u ) load block n

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
RND-TEST-BLOCK BLOCK DUP ALIGNED = -> TRUE
```

Source: `tests/forth-tests/blocktest.fth`

```forth
RND-TEST-BLOCK DUP BLOCK SWAP BLOCK = -> TRUE
```

Source: `tests/forth-tests/blocktest.fth`

```forth
RND-TEST-BLOCK DUP BLOCK SWAP BUFFER = -> TRUE
```

Source: `tests/forth-tests/blocktest.fth`

## SEE ALSO

- [`=`](_.md)
- [`BUFFER`](buffer.md)
- [`DUP`](dup.md)
- [`SWAP`](swap.md)
