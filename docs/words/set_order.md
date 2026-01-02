# SET-ORDER

## NAME

`SET-ORDER` — set the search order from stack

## SYNOPSIS

`SET-ORDER ( wid... count -- )`

## DESCRIPTION

SET-ORDER ( wid... count -- ) - set the search order from stack

## FLAGS

- Module: `(core)`
- Immediate: `True`
- Async: `False`

## EXAMPLES

```forth
GET-ORDER SET-ORDER ->
```

Source: `tests/forth-tests/searchordertest.fth`

```forth
GET-ORDERLIST DROP GET-ORDERLIST 2* SET-ORDER ->
```

Source: `tests/forth-tests/searchordertest.fth`

```forth
GET-ORDERLIST SET-ORDER GET-ORDER -> GET-ORDERLIST
```

Source: `tests/forth-tests/searchordertest.fth`

## SEE ALSO

- [`2*`](2_.md)
- [`DROP`](drop.md)
- [`GET-ORDER`](get_order.md)
