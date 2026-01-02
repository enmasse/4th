# R/W

## NAME

`R/W`

## SYNOPSIS

`R/W ( -- fam )`

## DESCRIPTION

R/W ( -- fam ) read-write file access method

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
FN1 R/W CREATE-FILE SWAP FID1 ! -> 0
```

Source: `tests/forth-tests/filetest.fth`

```forth
FN1 R/W OPEN-FILE SWAP FID1 ! -> 0
```

Source: `tests/forth-tests/filetest.fth`

```forth
FN2 R/W BIN CREATE-FILE SWAP FID2 ! -> 0
```

Source: `tests/forth-tests/filetest.fth`

## SEE ALSO

- [`!`](_.md)
- [`BIN`](bin.md)
- [`CREATE-FILE`](create_file.md)
- [`OPEN-FILE`](open_file.md)
- [`SWAP`](swap.md)
