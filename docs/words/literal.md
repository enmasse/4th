# LITERAL

## NAME

`LITERAL`

## SYNOPSIS

`LITERAL`

## DESCRIPTION

Compile a literal value into the current definition

## FLAGS

- Module: `(core)`
- Immediate: `True`
- Async: `False`

## EXAMPLES

```forth
: GC3 [ GC1 ] LITERAL ; ->
```

Source: `tests/forth-tests/core.fr`

```forth
: GT3 GT2 LITERAL ; ->
```

Source: `tests/forth-tests/core.fr`

```forth
: GT9 GT8 LITERAL ; ->
```

Source: `tests/forth-tests/core.fr`

## SEE ALSO

- [`:`](_.md)
- [`;`](_.md)
- [`[`](_.md)
- [`]`](_.md)
