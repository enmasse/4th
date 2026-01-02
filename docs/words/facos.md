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
1E facos 0E tf= -> true
```

Source: `tests/forth-tests/fp/ak-fp-test.fth`

```forth
0.5E facos pi f/ 0.3333E tf= -> true
```

Source: `tests/forth-tests/fp/ak-fp-test.fth`

```forth
0E facos pi f/ 0.5E tf= -> true
```

Source: `tests/forth-tests/fp/ak-fp-test.fth`

## SEE ALSO

- [`f/`](f_.md)
