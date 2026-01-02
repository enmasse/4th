# CR

## NAME

`CR`

## SYNOPSIS

`CR`

## DESCRIPTION

Emit newline

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
CR .( You should see -9876: ) -9876 . ->
```

Source: `tests/forth-tests/coreexttest.fth`

```forth
CR .( and again: ).( -9876)CR ->
```

Source: `tests/forth-tests/coreexttest.fth`

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
- [`EMIT`](emit.md)
- [`[`](_.md)
- [`[CHAR]`](_char_.md)
- [`]`](_.md)
