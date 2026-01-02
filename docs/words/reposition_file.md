# REPOSITION-FILE

## NAME

`REPOSITION-FILE` — seek to offset in file

## SYNOPSIS

`REPOSITION-FILE ( fid offset -- )`

## DESCRIPTION

REPOSITION-FILE ( fid offset -- ) - seek to offset in file

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
FID1 @ FILE-SIZE DROP FID1 @ REPOSITION-FILE -> 0
```

Source: `tests/forth-tests/filetest.fth`

```forth
10 0 FID1 @ REPOSITION-FILE -> 0
```

Source: `tests/forth-tests/filetest.fth`

```forth
0 0 FID1 @ REPOSITION-FILE -> 0
```

Source: `tests/forth-tests/filetest.fth`

## SEE ALSO

- [`@`](_.md)
- [`DROP`](drop.md)
- [`FILE-SIZE`](file_size.md)
