# :NONAME

## NAME

`:NONAME` — begin an anonymous definition, leaving xt on stack

## SYNOPSIS

`:NONAME`

## DESCRIPTION

:NONAME - begin an anonymous definition, leaving xt on stack

## FLAGS

- Module: `(core)`
- Immediate: `True`
- Async: `False`

## EXAMPLES

```forth
:NONAME ( n -- 0,1,..n ) DUP IF DUP >R 1- RECURSE R> THEN ;
CONSTANT RN1 ->
```

Source: `tests/forth-tests/coreexttest.fth`

```forth
:NONAME [ 345 ] IW3 [ ! ] ; DROP IW3 @ -> 345
```

Source: `tests/forth-tests/coreplustest.fth`

```forth
:NONAME IW5 [ @ IW3 ! ] ; DROP IW3 @ -> 456
```

Source: `tests/forth-tests/coreplustest.fth`

## SEE ALSO

- [`!`](_.md)
- [`1-`](1_.md)
- [`;`](_.md)
- [`>R`](_r.md)
- [`@`](_.md)
- [`CONSTANT`](constant.md)
- [`DROP`](drop.md)
- [`DUP`](dup.md)
- [`IF`](if.md)
- [`R>`](r_.md)
- [`RECURSE`](recurse.md)
- [`THEN`](then.md)
