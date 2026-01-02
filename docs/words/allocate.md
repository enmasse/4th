# ALLOCATE

## NAME

`ALLOCATE` — allocate u bytes of memory

## SYNOPSIS

`ALLOCATE ( u -- a-addr ior )`

## DESCRIPTION

ALLOCATE ( u -- a-addr ior ) - allocate u bytes of memory

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
100 ALLOCATE SWAP ADDR1 ! -> 0
```

Source: `tests/forth-tests/memorytest.fth`

```forth
99 ALLOCATE SWAP ADDR1 ! -> 0
```

Source: `tests/forth-tests/memorytest.fth`

```forth
50 CHARS ALLOCATE SWAP ADDR1 ! -> 0
```

Source: `tests/forth-tests/memorytest.fth`

## SEE ALSO

- [`!`](_.md)
- [`CHARS`](chars.md)
- [`SWAP`](swap.md)
