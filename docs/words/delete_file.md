# DELETE-FILE

## NAME

`DELETE-FILE` — delete file

## SYNOPSIS

`DELETE-FILE ( c-addr u -- ior )`

## DESCRIPTION

DELETE-FILE ( c-addr u -- ior ) - delete file

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
FN2 DELETE-FILE -> 0
```

Source: `tests/forth-tests/filetest.fth`

```forth
FN2 DELETE-FILE 0= -> FALSE
```

Source: `tests/forth-tests/filetest.fth`

```forth
FN3 DELETE-FILE DROP ->
```

Source: `tests/forth-tests/filetest.fth`

## SEE ALSO

- [`0=`](0_.md)
- [`DROP`](drop.md)
