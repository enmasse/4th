# OPEN-FILE

## NAME

`OPEN-FILE` — open file, mode 0=read 1=write 2=append

## SYNOPSIS

`OPEN-FILE ( filename mode -- ior fid )`

## DESCRIPTION

OPEN-FILE ( filename mode -- ior fid ) - open file, mode 0=read 1=write 2=append

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
FN1 R/O OPEN-FILE SWAP FID1 ! -> 0
```

Source: `tests/forth-tests/filetest.fth`

```forth
FN1 R/O OPEN-FILE SWAP FID1 ! -> 0
```

Source: `tests/forth-tests/filetest.fth`

## SEE ALSO

- [`!`](_.md)
- [`R/O`](r_o.md)
- [`SWAP`](swap.md)
- [`W/O`](w_o.md)
