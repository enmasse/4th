# DUP

## NAME

`DUP`

## SYNOPSIS

`DUP ( x -- x x )`

## DESCRIPTION

Duplicate top stack item ( x -- x x )

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
RND-TEST-BLOCK BUFFER DUP ALIGNED = -> TRUE
```

Source: `tests/forth-tests/blocktest.fth`

## SEE ALSO

- [`=`](_.md)
- [`BLOCK`](block.md)
- [`BUFFER`](buffer.md)
- [`SWAP`](swap.md)
