# COMPARE

## NAME

`COMPARE` — compare two strings lexicographically, n=-1 less, 0 equal, 1 greater

## SYNOPSIS

`COMPARE ( c-addr1 u1 c-addr2 u2 -- n )`

## DESCRIPTION

COMPARE ( c-addr1 u1 c-addr2 u2 -- n ) - compare two strings lexicographically, n=-1 less, 0 equal, 1 greater

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
s" 10000" fbuf 5 compare -> 0
```

Source: `tests/forth-tests/fp/ak-fp-test.fth`

```forth
s" 10000" fbuf 5 compare -> 0
```

Source: `tests/forth-tests/fp/ak-fp-test.fth`

```forth
s" 33333" fbuf 5 compare -> 0
```

Source: `tests/forth-tests/fp/ak-fp-test.fth`

## SEE ALSO

- (none yet)
