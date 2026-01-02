# SOURCE

## NAME

`SOURCE` — return address and length of current input buffer

## SYNOPSIS

`SOURCE ( -- addr u )`

## DESCRIPTION

SOURCE ( -- addr u ) - return address and length of current input buffer

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
SOURCE DROP 0 >= >IN -> TRUE 0
```

Source: `tests/forth/io-source-tests.tester.4th`

## SEE ALSO

- [`>=`](__.md)
- [`>IN`](_in.md)
- [`DROP`](drop.md)
