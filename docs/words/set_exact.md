# SET-EXACT

## NAME

`SET-EXACT` — enable exact FP equality (compatibility no-op)

## SYNOPSIS

`SET-EXACT (compatibility no-op)`

## DESCRIPTION

SET-EXACT - enable exact FP equality (compatibility no-op)

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
DECIMAL
SET-EXACT
t{  S" ."    >FLOAT  ->   FALSE     }t
```

Source: `tests/forth-tests/fp/to-float-test.4th`

```forth
DECIMAL
SET-EXACT
t{  S" ."    >FLOAT  ->   FALSE     }t
```

Source: `tests/forth2012-test-suite-local/src/fp/to-float-test.4th`

```forth
DECIMAL
SET-EXACT
t{  S" ."    >FLOAT  ->   FALSE     }t
```

Source: `tests/forth2012-test-suite/src/fp/to-float-test.4th`

## SEE ALSO

- [`."`](__.md)
- [`>FLOAT`](_float.md)
- [`DECIMAL`](decimal.md)
