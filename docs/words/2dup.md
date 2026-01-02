# 2DUP

## NAME

`2DUP`

## SYNOPSIS

`2DUP ( x y -- x y x y )`

## DESCRIPTION

Duplicate top two stack items ( x y -- x y x y )

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
lower upper 2DUP 2>R prng PRNG-RANDOM 2R> WITHIN -> TRUE
```

Source: `tests/forth-tests/blocktest.fth`

```forth
BLOCK-RND RND-TEST-BLOCK 2DUP TL1 LOAD = -> TRUE
```

Source: `tests/forth-tests/blocktest.fth`

```forth
BLOCK-RND FIRST-TEST-BLOCK 2DUP TL1 LOAD = -> TRUE
```

Source: `tests/forth-tests/blocktest.fth`

## SEE ALSO

- [`2>R`](2_r.md)
- [`2R>`](2r_.md)
- [`=`](_.md)
- [`LOAD`](load.md)
- [`WITHIN`](within.md)
