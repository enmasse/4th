# FORTH

## NAME

`FORTH` — push the FORTH sentinel for core dictionary

## SYNOPSIS

`FORTH ( -- wid )`

## DESCRIPTION

FORTH ( -- wid ) - push the FORTH sentinel for core dictionary

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
ONLY FORTH DEFINITIONS ->
```

Source: `tests/forth-tests/searchordertest.fth`

```forth
ONLY FORTH DEFINITIONS ORDER ->
```

Source: `tests/forth-tests/searchordertest.fth`

## SEE ALSO

- [`DEFINITIONS`](definitions.md)
- [`GET-ORDER`](get_order.md)
- [`ONLY`](only.md)
