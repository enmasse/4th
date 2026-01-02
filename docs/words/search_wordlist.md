# SEARCH-WORDLIST

## NAME

`SEARCH-WORDLIST` — search wordlist for word

## SYNOPSIS

`SEARCH-WORDLIST ( c-addr u wid -- 0 | xt 1 | xt -1 )`

## DESCRIPTION

SEARCH-WORDLIST ( c-addr u wid -- 0 | xt 1 | xt -1 ) - search wordlist for word

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
$" DUP" WID1 @ SEARCH-WORDLIST -> XT  @ -1
```

Source: `tests/forth-tests/searchordertest.fth`

```forth
$" ("   WID1 @ SEARCH-WORDLIST -> XTI @  1
```

Source: `tests/forth-tests/searchordertest.fth`

```forth
$" DUP" WID2 @ SEARCH-WORDLIST ->        0
```

Source: `tests/forth-tests/searchordertest.fth`

## SEE ALSO

- [`@`](_.md)
