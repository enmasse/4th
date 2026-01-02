# CHAR+

## NAME

`CHAR+` — add char size to address

## SYNOPSIS

`CHAR+ ( c-addr -- c-addr+1 )`

## DESCRIPTION

CHAR+ ( c-addr -- c-addr+1 ) - add char size to address

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
1STC CHAR+ -> 2NDC
```

Source: `tests/forth-tests/core.fr`

```forth
2 A-ADDR CHAR+ C!  A-ADDR CHAR+ C@ -> 2
```

Source: `tests/forth-tests/core.fr`

```forth
GC4 DROP DUP C@ SWAP CHAR+ C@ -> 58 59
```

Source: `tests/forth-tests/core.fr`

## SEE ALSO

- [`C!`](c_.md)
- [`C@`](c_.md)
- [`DROP`](drop.md)
- [`DUP`](dup.md)
- [`SWAP`](swap.md)
