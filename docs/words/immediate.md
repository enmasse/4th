# IMMEDIATE

## NAME

`IMMEDIATE`

## SYNOPSIS

`IMMEDIATE`

## DESCRIPTION

Mark the most recently defined word as immediate

## FLAGS

- Module: `(core)`
- Immediate: `True`
- Async: `False`

## EXAMPLES

```forth
: GT2 ['] GT1 ; IMMEDIATE ->
```

Source: `tests/forth-tests/core.fr`

```forth
: GT4 POSTPONE GT1 ; IMMEDIATE ->
```

Source: `tests/forth-tests/core.fr`

```forth
: GT6 345 ; IMMEDIATE ->
```

Source: `tests/forth-tests/core.fr`

## SEE ALSO

- [`:`](_.md)
- [`;`](_.md)
- [`POSTPONE`](postpone.md)
- [`[']`](___.md)
