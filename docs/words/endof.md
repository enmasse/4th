# ENDOF

## NAME

`ENDOF` — end a case branch

## SYNOPSIS

`ENDOF`

## DESCRIPTION

ENDOF - end a case branch

## FLAGS

- Module: `(core)`
- Immediate: `True`
- Async: `False`

## EXAMPLES

```forth
: CS6 CASE 1 OF ENDOF 2 ENDCASE ; 1 CS6 ->
```

Source: `tests/forth-tests/coreexttest.fth`

```forth
: CS7 CASE 3 OF ENDOF 2 ENDCASE ; 1 CS7 -> 1
```

Source: `tests/forth-tests/coreexttest.fth`

```forth
: CS6 CASE 1 OF ENDOF 2 ENDCASE ; 1 CS6 ->
```

Source: `tests/forth2012-test-suite/src/coreexttest.fth`

## SEE ALSO

- [`:`](_.md)
- [`;`](_.md)
- [`CASE`](case.md)
- [`ENDCASE`](endcase.md)
- [`OF`](of.md)
