# FSWAP

## NAME

`FSWAP` — swap top two floating items

## SYNOPSIS

`FSWAP ( r1 r2 -- r2 r1 )`

## DESCRIPTION

FSWAP ( r1 r2 -- r2 r1 ) - swap top two floating items

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
1. d>f 2. d>f fswap f>d f>d -> 1. 2.
```

Source: `tests/forth-tests/fp/ak-fp-test.fth`

```forth
1. d>f 2. d>f fswap f>d f>d -> 1. 2.
```

Source: `tests/forth2012-test-suite/src/fp/ak-fp-test.fth`

```forth
1. d>f 2. d>f fswap f>d f>d -> 1. 2.
```

Source: `tests/forth2012-test-suite-local/src/fp/ak-fp-test.fth`

## SEE ALSO

- [`d>f`](d_f.md)
- [`f>d`](f_d.md)
