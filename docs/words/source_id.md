# SOURCE-ID

## NAME

`SOURCE-ID` — return the input source identifier

## SYNOPSIS

`SOURCE-ID ( -- 0 | -1 | fileid )`

## DESCRIPTION

SOURCE-ID ( -- 0 | -1 | fileid ) - return the input source identifier

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
SOURCE-ID DUP -1 = SWAP 0= OR -> FALSE
```

Source: `tests/forth-tests/filetest.fth`

```forth
SOURCE-ID DUP -1 = SWAP 0= OR -> FALSE
```

Source: `tests/forth2012-test-suite/src/filetest.fth`

```forth
SOURCE-ID DUP -1 = SWAP 0= OR -> FALSE
```

Source: `tests/forth2012-test-suite-local/src/filetest.fth`

## SEE ALSO

- [`0=`](0_.md)
- [`=`](_.md)
- [`DUP`](dup.md)
- [`OR`](or.md)
- [`SWAP`](swap.md)
