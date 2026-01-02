# HOLD

## NAME

`HOLD`

## SYNOPSIS

`HOLD`

## DESCRIPTION

Hold a character for pictured numeric output

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
123 0 <# #S BL HOLD HTEST2 HOLDS BL HOLD HTEST HOLDS #>
HTEST3 S= -> TRUE
```

Source: `tests/forth-tests/coreexttest.fth`

```forth
123 0 <# #S BL HOLD HTEST2 HOLDS BL HOLD HTEST HOLDS #>
HTEST3 S= -> TRUE
```

Source: `tests/forth2012-test-suite/src/coreexttest.fth`

```forth
123 0 <# #S BL HOLD HTEST2 HOLDS BL HOLD HTEST HOLDS #>
HTEST3 S= -> TRUE
```

Source: `tests/forth2012-test-suite-local/src/coreexttest.fth`

## SEE ALSO

- [`#>`](__.md)
- [`#S`](_s.md)
- [`<#`](__.md)
- [`BL`](bl.md)
