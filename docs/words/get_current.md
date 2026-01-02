# GET-CURRENT

## NAME

`GET-CURRENT` — get the current compilation wordlist

## SYNOPSIS

`GET-CURRENT ( -- wid )`

## DESCRIPTION

GET-CURRENT ( -- wid ) - get the current compilation wordlist

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
GET-CURRENT -> WID1 @
```

Source: `tests/forth-tests/searchordertest.fth`

```forth
GET-CURRENT -> WID2 @
```

Source: `tests/forth-tests/searchordertest.fth`

```forth
GET-CURRENT -> FORTH-WORDLIST
```

Source: `tests/forth-tests/searchordertest.fth`

## SEE ALSO

- [`@`](_.md)
- [`FORTH-WORDLIST`](forth_wordlist.md)
