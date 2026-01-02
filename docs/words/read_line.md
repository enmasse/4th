# READ-LINE

## NAME

`READ-LINE` — read a line excluding CR/LF terminators

## SYNOPSIS

`READ-LINE ( c-addr u -- actual )`

## DESCRIPTION

READ-LINE ( c-addr u -- actual ) - read a line excluding CR/LF terminators

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
BUF 100 FID1 @ READ-LINE ROT DUP #CHARS ! -> TRUE 0 LINE1 SWAP DROP
```

Source: `tests/forth-tests/filetest.fth`

```forth
BUF 0 FID1 @ READ-LINE ROT DUP #CHARS ! -> TRUE 0 0
```

Source: `tests/forth-tests/filetest.fth`

```forth
BUF 3 FID1 @ READ-LINE ROT DUP #CHARS ! -> TRUE 0 3
```

Source: `tests/forth-tests/filetest.fth`

## SEE ALSO

- [`!`](_.md)
- [`@`](_.md)
- [`DROP`](drop.md)
- [`DUP`](dup.md)
- [`ROT`](rot.md)
- [`SWAP`](swap.md)
