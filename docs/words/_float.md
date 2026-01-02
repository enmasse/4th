# >FLOAT

## NAME

`>FLOAT` — attempt to convert string to floating-point number

## SYNOPSIS

`>FLOAT ( c-addr u -- r true | c-addr u false )`

## DESCRIPTION

>FLOAT ( c-addr u -- r true | c-addr u false ) - attempt to convert string to floating-point number

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
S" 3.14159E" >FLOAT -> -1E FACOS TRUE RX
```

Source: `tests/ttester.4th`

```forth
s" ." >float -> false
```

Source: `tests/forth-tests/fp/ak-fp-test.fth`

```forth
s" .E" >float -> false
```

Source: `tests/forth-tests/fp/ak-fp-test.fth`

## SEE ALSO

- [`."`](__.md)
- [`FACOS`](facos.md)
