# HEX

## NAME

`HEX`

## SYNOPSIS

`HEX`

## DESCRIPTION

Set number base to hexadecimal

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
HEX : LT20 {: BEAD DEAF :} DEAF BEAD ; BEEF DEAD LT20 -> DEAD BEEF
```

Source: `tests/forth-tests/localstest.fth`

## SEE ALSO

- [`!`](_.md)
- [`-`](_.md)
- [`:`](_.md)
- [`;`](_.md)
- [`@`](_.md)
- [`BASE`](base.md)
- [`d>f`](d_f.md)
- [`DECIMAL`](decimal.md)
- [`f>d`](f_d.md)
- [`SWAP`](swap.md)
