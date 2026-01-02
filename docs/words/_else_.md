# [ELSE]

## NAME

`[ELSE]` — conditional compilation alternate part

## SYNOPSIS

`[ELSE]`

## DESCRIPTION

[ELSE] - conditional compilation alternate part

## FLAGS

- Module: `(core)`
- Immediate: `True`
- Async: `False`

## EXAMPLES

```forth
TRUE  [IF] 111 [ELSE] 222 [THEN] -> 111
```

Source: `tests/forth-tests/toolstest.fth`

```forth
FALSE [IF] 111 [ELSE] 222 [THEN] -> 222
```

Source: `tests/forth-tests/toolstest.fth`

```forth
TRUE  [IF] 1     \ Code spread over more than 1 line
2
[ELSE]
3
4
[THEN] -> 1 2
```

Source: `tests/forth-tests/toolstest.fth`

## SEE ALSO

- [`[IF]`](_if_.md)
- [`[THEN]`](_then_.md)
