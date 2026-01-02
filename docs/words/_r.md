# >R

## NAME

`>R` — push top of data stack onto return stack

## SYNOPSIS

`>R ( x -- )`

## DESCRIPTION

>R ( x -- ) - push top of data stack onto return stack

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
: GR1 >R R> ; ->
```

Source: `tests/forth-tests/core.fr`

```forth
: GR2 >R R@ R> DROP ; ->
```

Source: `tests/forth-tests/core.fr`

```forth
: GI6 ( N -- 0,1,..N ) DUP IF DUP >R 1- RECURSE R> THEN ; ->
```

Source: `tests/forth-tests/core.fr`

## SEE ALSO

- [`1-`](1_.md)
- [`:`](_.md)
- [`;`](_.md)
- [`DROP`](drop.md)
- [`DUP`](dup.md)
- [`IF`](if.md)
- [`R>`](r_.md)
- [`R@`](r_.md)
- [`RECURSE`](recurse.md)
- [`THEN`](then.md)
