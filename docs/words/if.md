# IF

## NAME

`IF`

## SYNOPSIS

`IF`

## DESCRIPTION

Begin an if-then construct

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
- [`THEN`](then.md)
