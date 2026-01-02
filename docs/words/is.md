# IS

## NAME

`IS` — set a deferred to an execution token

## SYNOPSIS

`IS`

## DESCRIPTION

IS <name> - set a deferred to an execution token

## FLAGS

- Module: `(core)`
- Immediate: `True`
- Async: `False`

## EXAMPLES

```forth
: IS-DEFER1 IS DEFER1 ; ->
```

Source: `tests/forth-tests/coreexttest.fth`

```forth
' + IS DEFER1 ->
```

Source: `tests/forth-tests/coreexttest.fth`

```forth
' DUP IS DEFER2 ->
```

Source: `tests/forth-tests/coreexttest.fth`

## SEE ALSO

- [`'`](_.md)
- [`+`](_.md)
- [`:`](_.md)
- [`;`](_.md)
- [`DUP`](dup.md)
