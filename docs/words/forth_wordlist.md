# FORTH-WORDLIST

## NAME

`FORTH-WORDLIST` — push the wordlist id for the FORTH wordlist

## SYNOPSIS

`FORTH-WORDLIST ( -- wid )`

## DESCRIPTION

FORTH-WORDLIST ( -- wid ) - push the wordlist id for the FORTH wordlist

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
FORTH-WORDLIST WID1 ! ->
```

Source: `tests/forth-tests/searchordertest.fth`

```forth
ONLY FORTH-WORDLIST 1 SET-ORDER GET-ORDERLIST SO1 ->
```

Source: `tests/forth-tests/searchordertest.fth`

```forth
GET-CURRENT -> FORTH-WORDLIST
```

Source: `tests/forth-tests/searchordertest.fth`

## SEE ALSO

- [`!`](_.md)
- [`GET-CURRENT`](get_current.md)
- [`ONLY`](only.md)
- [`SET-ORDER`](set_order.md)
