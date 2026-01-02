# CMOVE>

## NAME

`CMOVE>` — copy u bytes from c-addr1 to c-addr2, proceeding from high to low

## SYNOPSIS

`CMOVE> ( c-addr1 c-addr2 u -- )`

## DESCRIPTION

CMOVE> ( c-addr1 c-addr2 u -- ) - copy u bytes from c-addr1 to c-addr2, proceeding from high to low

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
S1 PAD SWAP CMOVE> ->
```

Source: `tests/forth-tests/stringtest.fth`

```forth
S6 PAD 10 CHARS + SWAP CMOVE> ->
```

Source: `tests/forth-tests/stringtest.fth`

```forth
PAD 15 CHARS + PAD CHAR+ 6 CMOVE> ->
```

Source: `tests/forth-tests/stringtest.fth`

## SEE ALSO

- [`+`](_.md)
- [`CHAR+`](char_.md)
- [`CHARS`](chars.md)
- [`PAD`](pad.md)
- [`SWAP`](swap.md)
