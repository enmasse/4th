# DEFER

## NAME

`DEFER` — define a deferred execution token

## SYNOPSIS

`DEFER`

## DESCRIPTION

DEFER <name> - define a deferred execution token

## FLAGS

- Module: `(core)`
- Immediate: `True`
- Async: `False`

## EXAMPLES

```forth
DEFER DEFER1 ->
```

Source: `tests/forth-tests/coreexttest.fth`

```forth
: MY-DEFER DEFER ; ->
```

Source: `tests/forth-tests/coreexttest.fth`

```forth
DEFER DEFER1 ->
```

Source: `tests/forth2012-test-suite/src/coreexttest.fth`

## SEE ALSO

- [`:`](_.md)
- [`;`](_.md)
