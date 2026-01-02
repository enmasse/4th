# DEFER@

## NAME

`DEFER@` — get the execution token of the most recently defined deferred word

## SYNOPSIS

`DEFER@ ( -- xt )`

## DESCRIPTION

DEFER@ ( -- xt ) - get the execution token of the most recently defined deferred word

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
: DEF@ DEFER@ ; ->
```

Source: `tests/forth-tests/coreexttest.fth`

```forth
' DEFER1 DEFER@ -> ' *
```

Source: `tests/forth-tests/coreexttest.fth`

```forth
' DEFER1 DEFER@ -> ' +
```

Source: `tests/forth-tests/coreexttest.fth`

## SEE ALSO

- [`'`](_.md)
- [`*`](_.md)
- [`+`](_.md)
- [`:`](_.md)
- [`;`](_.md)
