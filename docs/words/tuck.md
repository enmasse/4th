# TUCK

## NAME

`TUCK`

## SYNOPSIS

`TUCK ( a b -- b a b )`

## DESCRIPTION

TUCK ( a b -- b a b ) copy top under second

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
1 2 TUCK -> 2 1 2
```

Source: `tests/forth-tests/coreexttest.fth`

```forth
1 2 3 TUCK -> 1 3 2 3
```

Source: `tests/forth-tests/coreexttest.fth`

```forth
14145 8115 ?DUP 0= 34 AND >IN +! TUCK MOD 14 >IN ! GCD CALCULATION -> 15
```

Source: `tests/forth-tests/coreplustest.fth`

## SEE ALSO

- [`!`](_.md)
- [`+!`](__.md)
- [`0=`](0_.md)
- [`>IN`](_in.md)
- [`?DUP`](_dup.md)
- [`AND`](and.md)
- [`MOD`](mod.md)
