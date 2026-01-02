# WRITE-FILE

## NAME

`WRITE-FILE` — write string data to file

## SYNOPSIS

`WRITE-FILE ( c-addr u filename | string string | counted-addr string -- )`

## DESCRIPTION

WRITE-FILE ( c-addr u filename | string string | counted-addr string -- ) - write string data to file

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
LINE2 FID1 @ WRITE-FILE -> 0
```

Source: `tests/forth-tests/filetest.fth`

```forth
PAD 50 FID2 @ WRITE-FILE FID2 @ FLUSH-FILE -> 0 0
```

Source: `tests/forth-tests/filetest.fth`

```forth
LINE2 FID1 @ WRITE-FILE -> 0
```

Source: `tests/forth2012-test-suite-local/src/filetest.fth`

## SEE ALSO

- [`@`](_.md)
- [`PAD`](pad.md)
