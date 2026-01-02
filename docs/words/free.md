# FREE

## NAME

`FREE` — deallocate memory at a-addr

## SYNOPSIS

`FREE ( a-addr -- ior )`

## DESCRIPTION

FREE ( a-addr -- ior ) - deallocate memory at a-addr

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
ADDR1 @ FREE -> 0
```

Source: `tests/forth-tests/memorytest.fth`

```forth
ADDR1 @ FREE -> 0
```

Source: `tests/forth-tests/memorytest.fth`

```forth
ADDR1 @ FREE -> 0
```

Source: `tests/forth-tests/memorytest.fth`

## SEE ALSO

- [`@`](_.md)
