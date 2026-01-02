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
S1 S2 SEARCH -> S1 TRUE
```

Source: `tests/forth-tests/stringtest.fth`

```forth
S1 S3 SEARCH -> S1  9 /STRING TRUE
```

Source: `tests/forth-tests/stringtest.fth`

```forth
S1 S4 SEARCH -> S1 25 /STRING TRUE
```

Source: `tests/forth-tests/stringtest.fth`

## SEE ALSO

- [`/STRING`](_string.md)
