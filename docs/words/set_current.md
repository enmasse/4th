# SET-CURRENT

## NAME

`SET-CURRENT` — set the current compilation wordlist

## SYNOPSIS

`SET-CURRENT ( wid -- )`

## DESCRIPTION

SET-CURRENT ( wid -- ) - set the current compilation wordlist

## FLAGS

- Module: `(core)`
- Immediate: `True`
- Async: `False`

## EXAMPLES

```forth
WID2 @ SET-CURRENT ->
```

Source: `tests/forth-tests/searchordertest.fth`

```forth
WID1 @ SET-CURRENT ->
```

Source: `tests/forth-tests/searchordertest.fth`

```forth
WID2 @ SET-CURRENT ->
```

Source: `tests/forth2012-test-suite/src/searchordertest.fth`

## SEE ALSO

- [`@`](_.md)
