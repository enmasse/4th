# SEE

## NAME

`SEE` — display decompiled definition or placeholder

## SYNOPSIS

`SEE`

## DESCRIPTION

SEE <name> - display decompiled definition or placeholder

## FLAGS

- Module: `(core)`
- Immediate: `True`
- Async: `False`

## EXAMPLES

```forth
: PB1 CR ." You should see 2345: "." 2345"( A comment) CR ; PB1 ->
```

Source: `tests/forth-tests/coreplustest.fth`

```forth
: PB1 CR ." You should see 2345: "." 2345"( A comment) CR ; PB1 ->
```

Source: `tests/forth2012-test-suite/src/coreplustest.fth`

```forth
: PB1 CR ." You should see 2345: "." 2345"( A comment) CR ; PB1 ->
```

Source: `tests/forth2012-test-suite-local/src/coreplustest.fth`

## SEE ALSO

- [`."`](__.md)
- [`:`](_.md)
- [`;`](_.md)
- [`CR`](cr.md)
