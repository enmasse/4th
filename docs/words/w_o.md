# W/O

## NAME

`W/O`

## SYNOPSIS

`W/O ( -- fam )`

## DESCRIPTION

W/O ( -- fam ) write-only file access method

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
FN1 W/O OPEN-FILE SWAP FID1 ! -> 0
```

Source: `tests/forth-tests/filetest.fth`

```forth
FN1 W/O OPEN-FILE SWAP FID1 ! -> 0
```

Source: `tests/forth2012-test-suite/src/filetest.fth`

```forth
FN1 W/O OPEN-FILE SWAP FID1 ! -> 0
```

Source: `tests/forth2012-test-suite-local/src/filetest.fth`

## SEE ALSO

- [`!`](_.md)
- [`OPEN-FILE`](open_file.md)
- [`SWAP`](swap.md)
