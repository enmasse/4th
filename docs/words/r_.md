# R@

## NAME

`R@` — read top of return stack without removing

## SYNOPSIS

`R@ ( -- x )`

## DESCRIPTION

R@ ( -- x ) - read top of return stack without removing

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
: GR2 >R R@ R> DROP ; ->
```

Source: `tests/forth-tests/core.fr`

```forth
: CHECKMEM  ( ad n --- )
0
DO
>R
T{ R@ C@ -> R> I 1+ SWAP >R
```

Source: `tests/forth-tests/memorytest.fth`

```forth
R> ( I ) -> R@ ( ADDR ) @
```

Source: `tests/forth-tests/memorytest.fth`

## SEE ALSO

- [`1+`](1_.md)
- [`:`](_.md)
- [`;`](_.md)
- [`>R`](_r.md)
- [`@`](_.md)
- [`C@`](c_.md)
- [`DO`](do.md)
- [`DROP`](drop.md)
- [`I`](i.md)
- [`R>`](r_.md)
- [`SWAP`](swap.md)
