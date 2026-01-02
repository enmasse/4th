# VALUE

## NAME

`VALUE` — define a named variable-like value (consumes stack)

## SYNOPSIS

`VALUE (consumes stack)`

## DESCRIPTION

VALUE <name> - define a named variable-like value (consumes stack)

## FLAGS

- Module: `(core)`
- Immediate: `True`
- Async: `False`

## EXAMPLES

```forth
111 VALUE VAL1 -999 VALUE VAL2 ->
```

Source: `tests/forth-tests/coreexttest.fth`

```forth
123 VALUE VAL3 IMMEDIATE VAL3 -> 123
```

Source: `tests/forth-tests/coreexttest.fth`

```forth
111 VALUE VAL1 -999 VALUE VAL2 ->
```

Source: `tests/forth2012-test-suite-local/src/coreexttest.fth`

## SEE ALSO

- [`IMMEDIATE`](immediate.md)
