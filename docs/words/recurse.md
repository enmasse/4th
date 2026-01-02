# RECURSE

## NAME

`RECURSE`

## SYNOPSIS

`RECURSE (supports recursive definitions)`

## DESCRIPTION

Compile a call to the current definition (supports recursive definitions)

## FLAGS

- Module: `(core)`
- Immediate: `True`
- Async: `False`

## EXAMPLES

```forth
: GI6 ( N -- 0,1,..N ) DUP IF DUP >R 1- RECURSE R> THEN ; ->
```

Source: `tests/forth-tests/core.fr`

```forth
:NONAME ( n -- 0,1,..n ) DUP IF DUP >R 1- RECURSE R> THEN ;
CONSTANT RN1 ->
```

Source: `tests/forth-tests/coreexttest.fth`

```forth
: LT30 {: A B :} A 0> IF A B * A 1- B 10 * RECURSE A B THEN ; ->
```

Source: `tests/forth-tests/localstest.fth`

## SEE ALSO

- [`*`](_.md)
- [`0>`](0_.md)
- [`1-`](1_.md)
- [`:`](_.md)
- [`:NONAME`](_noname.md)
- [`;`](_.md)
- [`>R`](_r.md)
- [`CONSTANT`](constant.md)
- [`DUP`](dup.md)
- [`IF`](if.md)
- [`R>`](r_.md)
- [`THEN`](then.md)
