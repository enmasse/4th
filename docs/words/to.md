# TO

## NAME

`TO` — set a VALUE to the top of stack

## SYNOPSIS

`TO`

## DESCRIPTION

TO <name> - set a VALUE to the top of stack

## FLAGS

- Module: `(core)`
- Immediate: `True`
- Async: `False`

## EXAMPLES

```forth
222 TO VAL1 ->
```

Source: `tests/forth-tests/coreexttest.fth`

```forth
: VD2 TO VAL2 ; ->
```

Source: `tests/forth-tests/coreexttest.fth`

```forth
444 TO VAL1 ->
```

Source: `tests/forth-tests/coreexttest.fth`

## SEE ALSO

- [`:`](_.md)
- [`;`](_.md)
