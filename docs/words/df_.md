# DF@

## NAME

`DF@` — fetch double-precision float (64-bit) from address

## SYNOPSIS

`DF@ ( addr -- r )`

## DESCRIPTION

DF@ ( addr -- r ) - fetch double-precision float (64-bit) from address

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
fmem df@ f>d -> 3.
```

Source: `tests/forth-tests/fp/ak-fp-test.fth`

```forth
fmem df@ f>d -> 3.
```

Source: `tests/forth2012-test-suite/src/fp/ak-fp-test.fth`

```forth
fmem df@ f>d -> 3.
```

Source: `tests/forth2012-test-suite-local/src/fp/ak-fp-test.fth`

## SEE ALSO

- [`f>d`](f_d.md)
