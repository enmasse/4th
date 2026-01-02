# ALSO

## NAME

`ALSO` — duplicate the first wordlist in the search order

## SYNOPSIS

`ALSO ( -- )`

## DESCRIPTION

ALSO ( -- ) - duplicate the first wordlist in the search order

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
ALSO GET-ORDER -> GET-ORDERLIST OVER SWAP 1+
```

Source: `tests/forth-tests/searchordertest.fth`

```forth
ALSO GET-ORDER -> GET-ORDERLIST OVER SWAP 1+
```

Source: `tests/forth2012-test-suite-local/src/searchordertest.fth`

```forth
ALSO GET-ORDER -> GET-ORDERLIST OVER SWAP 1+
```

Source: `tests/forth2012-test-suite/src/searchordertest.fth`

## SEE ALSO

- [`1+`](1_.md)
- [`GET-ORDER`](get_order.md)
- [`OVER`](over.md)
- [`SWAP`](swap.md)
