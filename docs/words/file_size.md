# FILE-SIZE

## NAME

`FILE-SIZE` — file size in bytes or -1 if error

## SYNOPSIS

`FILE-SIZE ( filename -- size | -1 )`

## DESCRIPTION

FILE-SIZE ( filename -- size | -1 ) - file size in bytes or -1 if error

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
FID1 @ FILE-SIZE -> FID1 @ FILE-POSITION
```

Source: `tests/forth-tests/filetest.fth`

```forth
FP 2@ FID1 @ FILE-SIZE DROP DEQ -> TRUE
```

Source: `tests/forth-tests/filetest.fth`

## SEE ALSO

- [`2@`](2_.md)
- [`@`](_.md)
- [`DROP`](drop.md)
- [`FILE-POSITION`](file_position.md)
- [`REPOSITION-FILE`](reposition_file.md)
