# FLOATS

## NAME

`FLOATS` — scale by float-cell size (8 bytes for double precision)

## SYNOPSIS

`FLOATS ( n -- n' )`

## DESCRIPTION

FLOATS ( n -- n' ) - scale by float-cell size (8 bytes for double precision)

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
0 floats -> 0
```

Source: `tests/forth-tests/fp/ak-fp-test.fth`

```forth
1 floats -> 8
```

Source: `tests/forth-tests/fp/ak-fp-test.fth`

```forth
-1 floats -> -8
```

Source: `tests/forth-tests/fp/ak-fp-test.fth`

## SEE ALSO

- (none yet)
