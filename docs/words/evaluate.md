# EVALUATE

## NAME

`EVALUATE` — interpret the string

## SYNOPSIS

`EVALUATE ( i*x c-addr u -- j*x )`

## DESCRIPTION

EVALUATE ( i*x c-addr u -- j*x ) - interpret the string

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
RND-TEST-BLOCK DUP TL3 DUP TL5 EVALUATE = -> TRUE
```

Source: `tests/forth-tests/blocktest.fth`

```forth
GE1 EVALUATE -> 123
```

Source: `tests/forth-tests/core.fr`

```forth
GE2 EVALUATE -> 124
```

Source: `tests/forth-tests/core.fr`

## SEE ALSO

- [`=`](_.md)
- [`DUP`](dup.md)
