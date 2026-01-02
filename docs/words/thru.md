# THRU

## NAME

`THRU`

## SYNOPSIS

`THRU ( n1 n2 -- )`

## DESCRIPTION

THRU ( n1 n2 -- ) load and interpret blocks from n1 to n2

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
2 RND-TEST-BLOCK-SEQ DUP TT1 DUP DUP 1+ THRU 1- = -> TRUE
```

Source: `tests/forth-tests/blocktest.fth`

```forth
2 RND-TEST-BLOCK-SEQ DUP TT1 DUP DUP 1+ THRU 1- = -> TRUE
```

Source: `tests/forth2012-test-suite/src/blocktest.fth`

```forth
2 RND-TEST-BLOCK-SEQ DUP TT1 DUP DUP 1+ THRU 1- = -> TRUE
```

Source: `tests/forth2012-test-suite-local/src/blocktest.fth`

## SEE ALSO

- [`1+`](1_.md)
- [`1-`](1_.md)
- [`=`](_.md)
- [`DUP`](dup.md)
