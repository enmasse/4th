# BIN

## NAME

`BIN`

## SYNOPSIS

`BIN ( fam -- fam' )`

## DESCRIPTION

BIN ( fam -- fam' ) modify fam for binary mode

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
FN2 R/W BIN CREATE-FILE SWAP FID2 ! -> 0
```

Source: `tests/forth-tests/filetest.fth`

```forth
FN2 R/W BIN OPEN-FILE SWAP FID2 ! -> 0
```

Source: `tests/forth-tests/filetest.fth`

```forth
FN2 R/W BIN OPEN-FILE SWAP DROP 0= -> FALSE
```

Source: `tests/forth-tests/filetest.fth`

## SEE ALSO

- [`!`](_.md)
- [`0=`](0_.md)
- [`CREATE-FILE`](create_file.md)
- [`DROP`](drop.md)
- [`OPEN-FILE`](open_file.md)
- [`R/W`](r_w.md)
- [`SWAP`](swap.md)
