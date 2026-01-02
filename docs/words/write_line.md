# WRITE-LINE

## NAME

`WRITE-LINE` — write u chars from c-addr to file, followed by newline

## SYNOPSIS

`WRITE-LINE ( c-addr u fileid -- )`

## DESCRIPTION

WRITE-LINE ( c-addr u fileid -- ) - write u chars from c-addr to file, followed by newline

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
LINE1 FID1 @ WRITE-LINE -> 0
```

Source: `tests/forth-tests/filetest.fth`

```forth
S" " FID1 @ WRITE-LINE -> 0
```

Source: `tests/forth-tests/filetest.fth`

```forth
S" " FID1 @ WRITE-LINE -> 0
```

Source: `tests/forth-tests/filetest.fth`

## SEE ALSO

- [`@`](_.md)
