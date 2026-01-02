# CONSTANT

## NAME

`CONSTANT` — define a constant with top value

## SYNOPSIS

`CONSTANT`

## DESCRIPTION

CONSTANT <name> - define a constant with top value

## FLAGS

- Module: `(core)`
- Immediate: `True`
- Async: `False`

## EXAMPLES

```forth
123 CONSTANT X123 ->
```

Source: `tests/forth-tests/core.fr`

```forth
: EQU CONSTANT ; ->
```

Source: `tests/forth-tests/core.fr`

```forth
:NONAME ( n -- 0,1,..n ) DUP IF DUP >R 1- RECURSE R> THEN ;
CONSTANT RN1 ->
```

Source: `tests/forth-tests/coreexttest.fth`

## SEE ALSO

- [`1-`](1_.md)
- [`:`](_.md)
- [`:NONAME`](_noname.md)
- [`;`](_.md)
- [`>R`](_r.md)
- [`DUP`](dup.md)
- [`IF`](if.md)
- [`R>`](r_.md)
- [`RECURSE`](recurse.md)
- [`THEN`](then.md)
