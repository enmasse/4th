# SEARCH

## NAME

`SEARCH` — search for substring

## SYNOPSIS

`SEARCH ( c-addr1 u1 c-addr2 u2 -- c-addr3 u3 flag )`

## DESCRIPTION

SEARCH ( c-addr1 u1 c-addr2 u2 -- c-addr3 u3 flag ) - search for substring

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
S" hello world" 11 S" world" 5 SEARCH DROP DROP -> -1
```

Source: `tests/forth/search-tests.tester.4th`

```forth
S" hello world" 11 S" foo" 3 SEARCH DROP DROP -> 0
```

Source: `tests/forth/search-tests.tester.4th`

```forth
S" abc" 3 S" a" 1 SEARCH DROP DROP -> -1
```

Source: `tests/forth/search-tests.tester.4th`

## SEE ALSO

- [`DROP`](drop.md)
