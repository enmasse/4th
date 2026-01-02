# ERASE

## NAME

`ERASE` — set u bytes at addr to zero

## SYNOPSIS

`ERASE ( addr u -- )`

## DESCRIPTION

ERASE ( addr u -- ) - set u bytes at addr to zero

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
PAD CHARS/PAD 2DUP CHARS ERASE 0 CHECKPAD -> TRUE
```

Source: `tests/forth-tests/coreexttest.fth`

```forth
PAD CHARS/PAD 2DUP MAXCHAR FILL PAD 0 ERASE MAXCHAR CHECKPAD -> TRUE
```

Source: `tests/forth-tests/coreexttest.fth`

```forth
PAD 43 CHARS + 9 CHARS ERASE ->
```

Source: `tests/forth-tests/coreexttest.fth`

## SEE ALSO

- [`+`](_.md)
- [`2DUP`](2dup.md)
- [`CHARS`](chars.md)
- [`FILL`](fill.md)
- [`PAD`](pad.md)
