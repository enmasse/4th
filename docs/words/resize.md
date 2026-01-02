# RESIZE

## NAME

`RESIZE` — resize allocated memory

## SYNOPSIS

`RESIZE ( a-addr1 u -- a-addr2 ior )`

## DESCRIPTION

RESIZE ( a-addr1 u -- a-addr2 ior ) - resize allocated memory

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
ADDR1 @ 28 CHARS RESIZE SWAP ADDR1 ! -> 0
```

Source: `tests/forth-tests/memorytest.fth`

```forth
ADDR1 @ 200 CHARS RESIZE SWAP ADDR1 ! -> 0
```

Source: `tests/forth-tests/memorytest.fth`

```forth
ADDR1 @ -1 CHARS RESIZE 0= DUP RESIZE-OK ! -> ADDR1 @ FALSE
```

Source: `tests/forth-tests/memorytest.fth`

## SEE ALSO

- [`!`](_.md)
- [`0=`](0_.md)
- [`@`](_.md)
- [`CHARS`](chars.md)
- [`DUP`](dup.md)
- [`SWAP`](swap.md)
