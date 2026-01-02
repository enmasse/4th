# [IF]

## NAME

`[IF]` — conditional compilation start (interpret/compile)

## SYNOPSIS

`[IF] ( flag -- )`

## DESCRIPTION

[IF] ( flag -- ) - conditional compilation start (interpret/compile)

## FLAGS

- Module: `(core)`
- Immediate: `True`
- Async: `False`

## EXAMPLES

```forth
SYSTEM_PREC SINGLE_PREC = [IF]
dec_t{  1.0e-10 !r -> }t
hex_t{  r4 L@  -> 2edbe6ff }t
```

Source: `tests/forth-tests/fp/fpio-test.4th`

```forth
TRUE  [IF] 111 [ELSE] 222 [THEN] -> 111
```

Source: `tests/forth-tests/toolstest.fth`

```forth
FALSE [IF] 111 [ELSE] 222 [THEN] -> 222
```

Source: `tests/forth-tests/toolstest.fth`

## SEE ALSO

- [`=`](_.md)
- [`[ELSE]`](_else_.md)
- [`[THEN]`](_then_.md)
