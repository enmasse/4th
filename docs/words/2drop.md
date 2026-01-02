# 2DROP

## NAME

`2DROP`

## SYNOPSIS

`2DROP ( a b -- )`

## DESCRIPTION

2DROP ( a b -- ) drop top two items

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
' TUF2-3 2RND-TEST-BLOCKS TUF2       \ run test procedure
2DROP 2= -> TRUE
```

Source: `tests/forth-tests/blocktest.fth`

```forth
' TUF2-EB 2RND-TEST-BLOCKS TUF2
2SWAP 2DROP 2= -> TRUE
```

Source: `tests/forth-tests/blocktest.fth`

## SEE ALSO

- [`'`](_.md)
- [`2SWAP`](2swap.md)
