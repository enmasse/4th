# M+

## NAME

`M+` — add single-cell n to double-cell d

## SYNOPSIS

`M+ ( d n -- d' )`

## DESCRIPTION

M+ ( d n -- d' ) - add single-cell n to double-cell d

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
HI-2INT   1 M+ -> HI-2INT   1. D+
```

Source: `tests/forth-tests/doubletest.fth`

```forth
MAX-2INT -1 M+ -> MAX-2INT -1. D+
```

Source: `tests/forth-tests/doubletest.fth`

```forth
MIN-2INT  1 M+ -> MIN-2INT  1. D+
```

Source: `tests/forth-tests/doubletest.fth`

## SEE ALSO

- [`D+`](d_.md)
