# -TRAILING

## NAME

`-TRAILING` — remove trailing spaces from string

## SYNOPSIS

`-TRAILING ( c-addr u1 -- c-addr u2 )`

## DESCRIPTION

-TRAILING ( c-addr u1 -- c-addr u2 ) - remove trailing spaces from string

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
S1 -TRAILING -> S1
```

Source: `tests/forth-tests/stringtest.fth`

```forth
S8 -TRAILING -> S8 2 -
```

Source: `tests/forth-tests/stringtest.fth`

```forth
S7 -TRAILING -> S7
```

Source: `tests/forth-tests/stringtest.fth`

## SEE ALSO

- [`-`](_.md)
