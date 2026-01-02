# COMPARE

## NAME

`COMPARE` — compare two strings lexicographically, n=-1 less, 0 equal, 1 greater

## SYNOPSIS

`COMPARE ( c-addr1 u1 c-addr2 u2 -- n )`

## DESCRIPTION

COMPARE ( c-addr1 u1 c-addr2 u2 -- n ) - compare two strings lexicographically, n=-1 less, 0 equal, 1 greater

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
S1 S1 COMPARE -> 0
```

Source: `tests/forth-tests/stringtest.fth`

```forth
S1 PAD OVER COMPARE -> 0
```

Source: `tests/forth-tests/stringtest.fth`

```forth
S1 PAD 6 COMPARE -> 1
```

Source: `tests/forth-tests/stringtest.fth`

## SEE ALSO

- [`OVER`](over.md)
- [`PAD`](pad.md)
