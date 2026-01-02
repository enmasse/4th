# SLITERAL

## NAME

`SLITERAL` — compile string literal

## SYNOPSIS

`SLITERAL ( c-addr1 u -- )`

## DESCRIPTION

SLITERAL ( c-addr1 u -- ) ( compiling: -- c-addr2 u ) - compile string literal

## FLAGS

- Module: `(core)`
- Immediate: `True`
- Async: `False`

## EXAMPLES

```forth
: S14 [ S1A ] SLITERAL ; ->
```

Source: `tests/forth-tests/stringtest.fth`

```forth
: S14 [ S1A ] SLITERAL ; ->
```

Source: `tests/forth2012-test-suite/src/stringtest.fth`

```forth
: S14 [ S1A ] SLITERAL ; ->
```

Source: `tests/forth2012-test-suite-local/src/stringtest.fth`

## SEE ALSO

- [`:`](_.md)
- [`;`](_.md)
- [`[`](_.md)
- [`]`](_.md)
