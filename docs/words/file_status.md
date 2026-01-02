# FILE-STATUS

## NAME

`FILE-STATUS` — get file status

## SYNOPSIS

`FILE-STATUS ( c-addr u -- x ior )`

## DESCRIPTION

FILE-STATUS ( c-addr u -- x ior ) - get file status

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
FN1 FILE-STATUS SWAP DROP 0= -> FALSE
```

Source: `tests/forth-tests/filetest.fth`

```forth
FN3 FILE-STATUS SWAP DROP 0= -> TRUE
```

Source: `tests/forth-tests/filetest.fth`

```forth
FN1 FILE-STATUS SWAP DROP 0= -> FALSE
```

Source: `tests/forth2012-test-suite/src/filetest.fth`

## SEE ALSO

- [`0=`](0_.md)
- [`DROP`](drop.md)
- [`SWAP`](swap.md)
