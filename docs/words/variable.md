# VARIABLE

## NAME

`VARIABLE` — define a named storage cell

## SYNOPSIS

`VARIABLE`

## DESCRIPTION

VARIABLE <name> - define a named storage cell

## FLAGS

- Module: `(core)`
- Immediate: `True`
- Async: `False`

## EXAMPLES

```forth
VARIABLE V1 ->
```

Source: `tests/forth-tests/core.fr`

```forth
VARIABLE IW3 IMMEDIATE 234 IW3 ! IW3 @ -> 234
```

Source: `tests/forth-tests/coreplustest.fth`

```forth
VARIABLE V1 ->
```

Source: `tests/forth2012-test-suite-local/src/core.fr`

## SEE ALSO

- [`!`](_.md)
- [`@`](_.md)
- [`IMMEDIATE`](immediate.md)
