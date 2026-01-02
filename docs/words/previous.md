# PREVIOUS

## NAME

`PREVIOUS` — remove the most recently added module from the search order

## SYNOPSIS

`PREVIOUS`

## DESCRIPTION

PREVIOUS - remove the most recently added module from the search order

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
: LT35 {: LT33 :} LT33 LTWL2 ALSO-LTWL LT33 PREVIOUS LT33 PREVIOUS LT33 ;
\?    ->
```

Source: `tests/forth-tests/localstest.fth`

```forth
PREVIOUS GET-ORDER -> GET-ORDERLIST
```

Source: `tests/forth-tests/searchordertest.fth`

```forth
PREVIOUS C"W2" FIND SO5 -> -1  1234
```

Source: `tests/forth-tests/searchordertest.fth`

## SEE ALSO

- [`:`](_.md)
- [`;`](_.md)
- [`FIND`](find.md)
- [`GET-ORDER`](get_order.md)
