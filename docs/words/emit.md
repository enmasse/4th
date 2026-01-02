# EMIT

## NAME

`EMIT`

## SYNOPSIS

`EMIT ( n -- )`

## DESCRIPTION

Emit character with given code ( n -- )

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
: DOTP  CR ." Second message via ." [CHAR] " EMIT    \ Check .( is immediate
[ CR ] .( First message via .( ) ; DOTP ->
```

Source: `tests/forth-tests/coreexttest.fth`

```forth
: DOTP  CR ." Second message via ." [CHAR] " EMIT    \ Check .( is immediate
[ CR ] .( First message via .( ) ; DOTP ->
```

Source: `tests/forth2012-test-suite-local/src/coreexttest.fth`

```forth
: DOTP  CR ." Second message via ." [CHAR] " EMIT    \ Check .( is immediate
[ CR ] .( First message via .( ) ; DOTP ->
```

Source: `tests/forth2012-test-suite/src/coreexttest.fth`

## SEE ALSO

- [`.`](_.md)
- [`."`](__.md)
- [`:`](_.md)
- [`;`](_.md)
- [`CR`](cr.md)
- [`[`](_.md)
- [`[CHAR]`](_char_.md)
- [`]`](_.md)
