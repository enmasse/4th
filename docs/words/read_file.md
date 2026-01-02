# READ-FILE

## NAME

`READ-FILE` — read entire file as string

## SYNOPSIS

`READ-FILE ( filename -- str )`

## DESCRIPTION

READ-FILE ( filename -- str ) - read entire file as string

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
CBUF BUF 29 FID2 @ READ-FILE -> 29 0
```

Source: `tests/forth-tests/filetest.fth`

```forth
CBUF BUF 29 FID2 @ READ-FILE -> 21 0
```

Source: `tests/forth-tests/filetest.fth`

```forth
BUF 10 FID2 @ READ-FILE -> 0 0
```

Source: `tests/forth-tests/filetest.fth`

## SEE ALSO

- [`@`](_.md)
