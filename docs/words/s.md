# S

## NAME

`S` — push string literal (string) or compile-time push

## SYNOPSIS

`S (string)`

## DESCRIPTION

S <text> - push string literal (string) or compile-time push

## FLAGS

- Module: `(core)`
- Immediate: `True`
- Async: `False`

## EXAMPLES

```forth
: SSQ1 S\" abc" S" abc" S= ; ->
```

Source: `tests/forth-tests/coreexttest.fth`

```forth
: SSQ2 S\" " ; SSQ2 SWAP DROP -> 0
```

Source: `tests/forth-tests/coreexttest.fth`

```forth
: SSQ3 S\" \a\b\e\f\l\m\q\r\t\v\x0F0\x1Fa\xaBx\z\"\\" ; ->
```

Source: `tests/forth-tests/coreexttest.fth`

## SEE ALSO

- [`:`](_.md)
