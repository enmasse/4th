# COUNT

## NAME

`COUNT` — return counted string address and length

## SYNOPSIS

`COUNT ( c-addr -- c-addr u )`

## DESCRIPTION

COUNT ( c-addr -- c-addr u ) - return counted string address and length

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
GT1STRING COUNT -> GT1STRING CHAR+ 3
```

Source: `tests/forth-tests/core.fr`

```forth
CQ1 COUNT EVALUATE -> 123
```

Source: `tests/forth-tests/coreexttest.fth`

```forth
CQ2 COUNT EVALUATE ->
```

Source: `tests/forth-tests/coreexttest.fth`

## SEE ALSO

- [`CHAR+`](char_.md)
- [`EVALUATE`](evaluate.md)
