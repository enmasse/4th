# BLANK

## NAME

`BLANK` — fill u bytes at addr with space

## SYNOPSIS

`BLANK ( addr u -- )`

## DESCRIPTION

BLANK ( addr u -- ) - fill u bytes at addr with space

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
PAD 5 CHARS + 6 BLANK ->
```

Source: `tests/forth-tests/stringtest.fth`

```forth
PAD 5 CHARS + 6 BLANK ->
```

Source: `tests/forth2012-test-suite-local/src/stringtest.fth`

```forth
PAD 5 CHARS + 6 BLANK ->
```

Source: `tests/forth2012-test-suite/src/stringtest.fth`

## SEE ALSO

- [`+`](_.md)
- [`CHARS`](chars.md)
- [`PAD`](pad.md)
