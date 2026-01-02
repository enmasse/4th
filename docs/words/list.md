# LIST

## NAME

`LIST`

## SYNOPSIS

`LIST ( n -- )`

## DESCRIPTION

LIST ( n -- ) display block n

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
RND-TEST-BLOCK DUP TLS1 DUP LIST SCR @ = -> TRUE
```

Source: `tests/forth-tests/blocktest.fth`

```forth
FIRST-TEST-BLOCK DUP TLS2 LIST ->
```

Source: `tests/forth-tests/blocktest.fth`

```forth
LIMIT-TEST-BLOCK 1- DUP TLS3 LIST ->
```

Source: `tests/forth-tests/blocktest.fth`

## SEE ALSO

- [`1-`](1_.md)
- [`=`](_.md)
- [`@`](_.md)
- [`DUP`](dup.md)
- [`SCR`](scr.md)
