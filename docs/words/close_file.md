# CLOSE-FILE

## NAME

`CLOSE-FILE` — close file handle

## SYNOPSIS

`CLOSE-FILE ( fid -- ior )`

## DESCRIPTION

CLOSE-FILE ( fid -- ior ) - close file handle

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
CLOSE-FILE ->
```

Source: `tests/forth/create-file-tests.4th`

```forth
FID1 @ CLOSE-FILE -> 0
```

Source: `tests/forth-tests/filetest.fth`

```forth
FID1 @ CLOSE-FILE -> 0
```

Source: `tests/forth-tests/filetest.fth`

## SEE ALSO

- [`@`](_.md)
