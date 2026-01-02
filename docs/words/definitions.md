# DEFINITIONS

## NAME

`DEFINITIONS` — set current compilation wordlist

## SYNOPSIS

`DEFINITIONS ( wid -- )`

## DESCRIPTION

DEFINITIONS ( wid -- ) - set current compilation wordlist

## FLAGS

- Module: `(core)`
- Immediate: `True`
- Async: `False`

## EXAMPLES

```forth
ONLY FORTH DEFINITIONS ->
```

Source: `tests/forth-tests/searchordertest.fth`

```forth
GET-ORDER WID2 @ SWAP 1+ SET-ORDER DEFINITIONS GET-CURRENT -> WID2 @
```

Source: `tests/forth-tests/searchordertest.fth`

```forth
DEFINITIONS GET-CURRENT -> FORTH-WORDLIST
```

Source: `tests/forth-tests/searchordertest.fth`

## SEE ALSO

- [`1+`](1_.md)
- [`@`](_.md)
- [`FORTH`](forth.md)
- [`FORTH-WORDLIST`](forth_wordlist.md)
- [`GET-CURRENT`](get_current.md)
- [`GET-ORDER`](get_order.md)
- [`ONLY`](only.md)
- [`SET-ORDER`](set_order.md)
- [`SWAP`](swap.md)
