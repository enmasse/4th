# CREATE-FILE

## NAME

`CREATE-FILE` — create file

## SYNOPSIS

`CREATE-FILE ( c-addr u fam -- fileid ior )`

## DESCRIPTION

CREATE-FILE ( c-addr u fam -- fileid ior ) - create file

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
S" testfile.txt" 1 CREATE-FILE SWAP DROP 0= -> TRUE
```

Source: `tests/forth/create-file-tests.4th`

```forth
FN1 R/W CREATE-FILE SWAP FID1 ! -> 0
```

Source: `tests/forth-tests/filetest.fth`

```forth
FN2 R/W BIN CREATE-FILE SWAP FID2 ! -> 0
```

Source: `tests/forth-tests/filetest.fth`

## SEE ALSO

- [`!`](_.md)
- [`0=`](0_.md)
- [`BIN`](bin.md)
- [`DROP`](drop.md)
- [`R/W`](r_w.md)
- [`SWAP`](swap.md)
