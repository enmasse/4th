# ENDCASE

## NAME

`ENDCASE` — end a CASE structure

## SYNOPSIS

`ENDCASE`

## DESCRIPTION

ENDCASE - end a CASE structure

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
- [`CASE`](case.md)
- [`ENDOF`](endof.md)
- [`OF`](of.md)
- [`SWAP`](swap.md)
