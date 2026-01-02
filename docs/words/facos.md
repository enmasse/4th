# FACOS

## NAME

`FACOS` — floating-point arccosine

## SYNOPSIS

`FACOS ( r -- r )`

## DESCRIPTION

FACOS ( r -- r ) - floating-point arccosine

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
1E facos 0E tf= -> true
```

Source: `tests/forth-tests/fp/ak-fp-test.fth`

```forth
0.5E facos pi f/ 0.3333E tf= -> true
```

Source: `tests/forth-tests/fp/ak-fp-test.fth`

## SEE ALSO

- [`>FLOAT`](_float.md)
- [`f/`](f_.md)
