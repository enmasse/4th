# ROT

## NAME

`ROT`

## SYNOPSIS

`ROT ( a b c -- b c a )`

## DESCRIPTION

Rotate third to top ( a b c -- b c a )

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
' TUF2-2 2RND-TEST-BLOCKS TUF2       \ run test procedure
DROP ROT DROP SWAP 2= -> TRUE
```

Source: `tests/forth-tests/blocktest.fth`

```forth
1 2 3 ROT -> 2 3 1
```

Source: `tests/forth-tests/core.fr`

```forth
RO5 2 ROLL -> RO5 ROT
```

Source: `tests/forth-tests/coreexttest.fth`

## SEE ALSO

- [`'`](_.md)
- [`DROP`](drop.md)
- [`SWAP`](swap.md)
