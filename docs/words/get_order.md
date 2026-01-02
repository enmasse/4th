# GET-ORDER

## NAME

`GET-ORDER` — push current search order list and count

## SYNOPSIS

`GET-ORDER ( -- wid... count )`

## DESCRIPTION

GET-ORDER ( -- wid... count ) - push current search order list and count

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
GET-ORDER SAVE-ORDERLIST ->
```

Source: `tests/forth-tests/searchordertest.fth`

```forth
GET-ORDER OVER -> GET-ORDER WID1 @
```

Source: `tests/forth-tests/searchordertest.fth`

```forth
GET-ORDER SET-ORDER ->
```

Source: `tests/forth-tests/searchordertest.fth`

## SEE ALSO

- [`@`](_.md)
- [`OVER`](over.md)
- [`SET-ORDER`](set_order.md)
