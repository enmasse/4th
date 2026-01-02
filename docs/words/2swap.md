# 2SWAP

## NAME

`2SWAP`

## SYNOPSIS

`2SWAP ( a b c d -- c d a b )`

## DESCRIPTION

Swap two pairs on the stack ( a b c d -- c d a b )

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
' TUF2-0 2RND-TEST-BLOCKS TUF2       \ run test procedure
2SWAP 2DROP 2= -> TRUE
```

Source: `tests/forth-tests/blocktest.fth`

```forth
' TUF2-EB 2RND-TEST-BLOCKS TUF2
2SWAP 2DROP 2= -> TRUE
```

Source: `tests/forth-tests/blocktest.fth`

```forth
1 2 3 4 2SWAP -> 3 4 1 2
```

Source: `tests/forth-tests/core.fr`

## SEE ALSO

- [`'`](_.md)
- [`2DROP`](2drop.md)
