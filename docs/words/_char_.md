# [CHAR]

## NAME

`[CHAR]` — compile character code literal

## SYNOPSIS

`[CHAR]`

## DESCRIPTION

[CHAR] <c> - compile character code literal

## FLAGS

- Module: `(core)`
- Immediate: `True`
- Async: `False`

## EXAMPLES

```forth
: GC1 [CHAR] X ; ->
```

Source: `tests/forth-tests/core.fr`

```forth
: GC2 [CHAR] HELLO ; ->
```

Source: `tests/forth-tests/core.fr`

```forth
: DOTP  CR ." Second message via ." [CHAR] " EMIT    \ Check .( is immediate
[ CR ] .( First message via .( ) ; DOTP ->
```

Source: `tests/forth-tests/coreexttest.fth`

## SEE ALSO

- [`.`](_.md)
- [`."`](__.md)
- [`:`](_.md)
- [`;`](_.md)
- [`CR`](cr.md)
- [`EMIT`](emit.md)
- [`[`](_.md)
- [`]`](_.md)
