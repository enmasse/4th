# FILE-POSITION

## NAME

`FILE-POSITION` — get current file position

## SYNOPSIS

`FILE-POSITION ( fileid -- ud ior )`

## DESCRIPTION

FILE-POSITION ( fileid -- ud ior ) - get current file position

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
FID1 @ FILE-POSITION -> 0 0 0
```

Source: `tests/forth-tests/filetest.fth`

```forth
FID1 @ FILE-POSITION -> 0 0 0
```

Source: `tests/forth-tests/filetest.fth`

```forth
FID1 @ FILE-POSITION -> 0 0 0
```

Source: `tests/forth-tests/filetest.fth`

## SEE ALSO

- [`@`](_.md)
