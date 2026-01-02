# SF@

## NAME

`SF@` — fetch single-precision float (32-bit) from address

## SYNOPSIS

`SF@ ( addr -- r )`

## DESCRIPTION

SF@ ( addr -- r ) - fetch single-precision float (32-bit) from address

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
fmem sf@ f>d -> -2.
```

Source: `tests/forth-tests/fp/ak-fp-test.fth`

```forth
fmem sf@ f>d -> -2.
```

Source: `tests/forth2012-test-suite/src/fp/ak-fp-test.fth`

```forth
fmem sf@ f>d -> -2.
```

Source: `tests/forth2012-test-suite-local/src/fp/ak-fp-test.fth`

## SEE ALSO

- [`f>d`](f_d.md)
