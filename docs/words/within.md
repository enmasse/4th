# WITHIN

## NAME

`WITHIN` — true if low <= x < high

## SYNOPSIS

`WITHIN ( x low high -- flag )`

## DESCRIPTION

WITHIN ( x low high -- flag ) - true if low <= x < high

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
RND-TEST-BLOCK FIRST-TEST-BLOCK LIMIT-TEST-BLOCK WITHIN -> TRUE
```

Source: `tests/forth-tests/blocktest.fth`

```forth
0 0 0 WITHIN -> FALSE
```

Source: `tests/forth-tests/coreexttest.fth`

## SEE ALSO

- [`2>R`](2_r.md)
- [`2DUP`](2dup.md)
- [`2R>`](2r_.md)
