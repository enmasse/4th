# R/O

## NAME

`R/O`

## SYNOPSIS

`R/O ( -- fam )`

## DESCRIPTION

R/O ( -- fam ) read-only file access method

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
FN1 R/O OPEN-FILE SWAP FID1 ! -> 0
```

Source: `tests/forth-tests/filetest.fth`

```forth
FN1 R/O OPEN-FILE SWAP FID1 ! -> 0
```

Source: `tests/forth-tests/filetest.fth`

```forth
FN1 R/O OPEN-FILE SWAP FID1 ! -> 0
```

Source: `tests/forth-tests/filetest.fth`

## SEE ALSO

- [`!`](_.md)
- [`OPEN-FILE`](open_file.md)
- [`SWAP`](swap.md)
