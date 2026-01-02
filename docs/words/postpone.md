# POSTPONE

## NAME

`POSTPONE` — compile semantics of a word

## SYNOPSIS

`POSTPONE`

## DESCRIPTION

POSTPONE <name> - compile semantics of a word

## FLAGS

- Module: `(core)`
- Immediate: `True`
- Async: `False`

## EXAMPLES

```forth
: GT4 POSTPONE GT1 ; IMMEDIATE ->
```

Source: `tests/forth-tests/core.fr`

```forth
: GT7 POSTPONE GT6 ; ->
```

Source: `tests/forth-tests/core.fr`

```forth
: NOP : POSTPONE ; ; ->
```

Source: `tests/forth-tests/core.fr`

## SEE ALSO

- [`:`](_.md)
- [`;`](_.md)
- [`IMMEDIATE`](immediate.md)
