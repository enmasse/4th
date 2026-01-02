# THEN

## NAME

`THEN`

## SYNOPSIS

`THEN`

## DESCRIPTION

End an if construct

## FLAGS

- Module: `(core)`
- Immediate: `True`
- Async: `False`

## EXAMPLES

```forth
: BITSSET? IF 0 0 ELSE 0 THEN ; ->
```

Source: `tests/forth-tests/core.fr`

```forth
: GI1 IF 123 THEN ; ->
```

Source: `tests/forth-tests/core.fr`

```forth
: GI2 IF 123 ELSE 234 THEN ; ->
```

Source: `tests/forth-tests/core.fr`

## SEE ALSO

- [`:`](_.md)
- [`;`](_.md)
- [`ELSE`](else.md)
- [`IF`](if.md)
