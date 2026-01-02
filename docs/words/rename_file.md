# RENAME-FILE

## NAME

`RENAME-FILE` — rename file

## SYNOPSIS

`RENAME-FILE ( c-addr1 u1 c-addr2 u2 -- ior )`

## DESCRIPTION

RENAME-FILE ( c-addr1 u1 c-addr2 u2 -- ior ) - rename file

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
FN1 FN3 RENAME-FILE 0= -> TRUE
```

Source: `tests/forth-tests/filetest.fth`

```forth
FN1 FN3 RENAME-FILE 0= -> TRUE
```

Source: `tests/forth2012-test-suite-local/src/filetest.fth`

```forth
FN1 FN3 RENAME-FILE 0= -> TRUE
```

Source: `tests/forth2012-test-suite/src/filetest.fth`

## SEE ALSO

- [`0=`](0_.md)
