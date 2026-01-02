# >IN

## NAME

`>IN` — address of >IN index cell

## SYNOPSIS

`>IN ( -- addr )`

## DESCRIPTION

>IN ( -- addr ) - address of >IN index cell

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
SOURCE DROP 0 >= >IN -> TRUE 0
```

Source: `tests/forth/io-source-tests.tester.4th`

```forth
12345 DEPTH OVER 9 < 34 AND + 3 + >IN ! -> 12345 2345 345 45 5
```

Source: `tests/forth-tests/coreplustest.fth`

```forth
14145 8115 ?DUP 0= 34 AND >IN +! TUCK MOD 14 >IN ! GCD CALCULATION -> 15
```

Source: `tests/forth-tests/coreplustest.fth`

## SEE ALSO

- [`!`](_.md)
- [`+`](_.md)
- [`+!`](__.md)
- [`0=`](0_.md)
- [`<`](_.md)
- [`>=`](__.md)
- [`?DUP`](_dup.md)
- [`AND`](and.md)
- [`DEPTH`](depth.md)
- [`DROP`](drop.md)
- [`MOD`](mod.md)
- [`OVER`](over.md)
