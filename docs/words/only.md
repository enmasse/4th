# ONLY

## NAME

`ONLY` — set the search order to the minimum wordlist (FORTH only)

## SYNOPSIS

`ONLY ( -- )`

## DESCRIPTION

ONLY ( -- ) - set the search order to the minimum wordlist (FORTH only)

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
ONLY FORTH GET-ORDER -> GET-ORDERLIST
```

Source: `tests/forth-tests/searchordertest.fth`

```forth
ONLY FORTH-WORDLIST 1 SET-ORDER GET-ORDERLIST SO1 ->
```

Source: `tests/forth-tests/searchordertest.fth`

```forth
ONLY FORTH DEFINITIONS ->
```

Source: `tests/forth-tests/searchordertest.fth`

## SEE ALSO

- [`DEFINITIONS`](definitions.md)
- [`FORTH`](forth.md)
- [`FORTH-WORDLIST`](forth_wordlist.md)
- [`GET-ORDER`](get_order.md)
- [`SET-ORDER`](set_order.md)
