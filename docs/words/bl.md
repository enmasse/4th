# BL

## NAME

`BL`

## SYNOPSIS

`BL ( -- 32 )`

## DESCRIPTION

BL ( -- 32 ) ASCII space character

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
BL -> 20
```

Source: `tests/forth-tests/core.fr`

```forth
BL GS3 HELLO -> 5 CHAR H
```

Source: `tests/forth-tests/core.fr`

```forth
BL GS3
DROP -> 0
```

Source: `tests/forth-tests/core.fr`

## SEE ALSO

- [`CHAR`](char.md)
- [`DROP`](drop.md)
