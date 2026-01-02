# CASE

## NAME

`CASE` — start a case structure ( sel -- sel )

## SYNOPSIS

`CASE ( sel -- sel )`

## DESCRIPTION

CASE - start a case structure ( sel -- sel )

## FLAGS

- Module: `(core)`
- Immediate: `True`
- Async: `False`

## EXAMPLES

```forth
: CS4 CASE ENDCASE ; 1 CS4 ->
```

Source: `tests/forth-tests/coreexttest.fth`

```forth
: CS5 CASE 2 SWAP ENDCASE ; 1 CS5 -> 2
```

Source: `tests/forth-tests/coreexttest.fth`

```forth
: CS6 CASE 1 OF ENDOF 2 ENDCASE ; 1 CS6 ->
```

Source: `tests/forth-tests/coreexttest.fth`

## SEE ALSO

- [`:`](_.md)
- [`;`](_.md)
- [`ENDCASE`](endcase.md)
- [`ENDOF`](endof.md)
- [`OF`](of.md)
- [`SWAP`](swap.md)
