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
FN1 R/W CREATE-FILE SWAP FID1 ! -> 0
```

Source: `tests/forth-tests/filetest.fth`

```forth
FN2 R/W BIN CREATE-FILE SWAP FID2 ! -> 0
```

Source: `tests/forth-tests/filetest.fth`

```forth
FN1 R/W CREATE-FILE SWAP FID1 ! -> 0
```

Source: `tests/forth2012-test-suite-local/src/filetest.fth`

## SEE ALSO

- [`!`](_.md)
- [`BIN`](bin.md)
- [`R/W`](r_w.md)
- [`SWAP`](swap.md)
