# EXECUTE

## NAME

`EXECUTE`

## SYNOPSIS

`EXECUTE`

## DESCRIPTION

Execute a word or execution token on the stack

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
' GT1 EXECUTE -> 123
```

Source: `tests/forth-tests/core.fr`

```forth
GT2 EXECUTE -> 123
```

Source: `tests/forth-tests/core.fr`

```forth
NN1 @ EXECUTE -> 1234
```

Source: `tests/forth-tests/coreexttest.fth`

## SEE ALSO

- [`'`](_.md)
- [`@`](_.md)
