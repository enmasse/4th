# /STRING

## NAME

`/STRING` — adjust string by n characters

## SYNOPSIS

`/STRING ( c-addr1 u1 n -- c-addr2 u2 )`

## DESCRIPTION

/STRING ( c-addr1 u1 n -- c-addr2 u2 ) - adjust string by n characters

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
BUF #CHARS @ LINE1 3 /STRING S= -> TRUE
```

Source: `tests/forth-tests/filetest.fth`

```forth
S1  5 /STRING -> S1 SWAP 5 + SWAP 5 -
```

Source: `tests/forth-tests/stringtest.fth`

```forth
S1 10 /STRING -4 /STRING -> S1 6 /STRING
```

Source: `tests/forth-tests/stringtest.fth`

## SEE ALSO

- [`+`](_.md)
- [`-`](_.md)
- [`@`](_.md)
- [`SWAP`](swap.md)
