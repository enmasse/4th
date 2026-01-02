# FDEPTH

## NAME

`FDEPTH` — return number of floating items on stack

## SYNOPSIS

`FDEPTH ( -- n )`

## DESCRIPTION

FDEPTH ( -- n ) - return number of floating items on stack

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
0. d>f fdepth f>d -> 1 0.
```

Source: `tests/forth-tests/fp/ak-fp-test.fth`

```forth
0. d>f fdrop fdepth -> 0
```

Source: `tests/forth-tests/fp/ak-fp-test.fth`

```forth
0. d>f 0. d>f fdrop fdepth f>d -> 1 0.
```

Source: `tests/forth-tests/fp/ak-fp-test.fth`

## SEE ALSO

- [`d>f`](d_f.md)
- [`f>d`](f_d.md)
- [`fdrop`](fdrop.md)
