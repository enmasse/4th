# DECIMAL

## NAME

`DECIMAL`

## SYNOPSIS

`DECIMAL`

## DESCRIPTION

Set number base to decimal

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
BASE @ HEX BASE @ DECIMAL BASE @ - SWAP BASE ! -> 6
```

Source: `tests/forth-tests/coreexttest.fth`

```forth
hex fffffffffffff. decimal d>f f>d -> hex fffffffffffff. decimal
```

Source: `tests/forth-tests/fp/ak-fp-test.fth`

```forth
DECIMAL
SET-EXACT
t{  S" ."    >FLOAT  ->   FALSE     }t
```

Source: `tests/forth-tests/fp/to-float-test.4th`

## SEE ALSO

- [`!`](_.md)
- [`-`](_.md)
- [`."`](__.md)
- [`>FLOAT`](_float.md)
- [`@`](_.md)
- [`BASE`](base.md)
- [`d>f`](d_f.md)
- [`f>d`](f_d.md)
- [`HEX`](hex.md)
- [`SET-EXACT`](set_exact.md)
- [`SWAP`](swap.md)
