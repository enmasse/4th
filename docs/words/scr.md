# SCR

## NAME

`SCR` — variable containing the block number most recently listed

## SYNOPSIS

`SCR ( -- addr )`

## DESCRIPTION

SCR ( -- addr ) - variable containing the block number most recently listed

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
SCR DUP ALIGNED = -> TRUE
```

Source: `tests/forth-tests/blocktest.fth`

```forth
RND-TEST-BLOCK DUP TLS1 DUP LIST SCR @ = -> TRUE
```

Source: `tests/forth-tests/blocktest.fth`

```forth
SCR @ FLUSH                                       SCR @ = -> TRUE
```

Source: `tests/forth-tests/blocktest.fth`

## SEE ALSO

- [`=`](_.md)
- [`@`](_.md)
- [`DUP`](dup.md)
- [`FLUSH`](flush.md)
- [`LIST`](list.md)
