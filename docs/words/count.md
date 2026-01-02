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
S" hello" COUNT SWAP DROP -> 5
```

Source: `tests/forth/memory-count-tests.tester.4th`

```forth
GT1STRING COUNT -> GT1STRING CHAR+ 3
```

Source: `tests/forth-tests/core.fr`

```forth
CQ1 COUNT EVALUATE -> 123
```

Source: `tests/forth-tests/coreexttest.fth`

## SEE ALSO

- [`CHAR+`](char_.md)
- [`DROP`](drop.md)
- [`EVALUATE`](evaluate.md)
- [`SWAP`](swap.md)
