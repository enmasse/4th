# FDROP

## NAME

`FDROP` — drop top floating item

## SYNOPSIS

`FDROP ( r -- )`

## DESCRIPTION

FDROP ( r -- ) - drop top floating item

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
0. d>f fdrop fdepth -> 0
```

Source: `tests/forth-tests/fp/ak-fp-test.fth`

```forth
0. d>f 0. d>f fdrop fdepth f>d -> 1 0.
```

Source: `tests/forth-tests/fp/ak-fp-test.fth`

```forth
0. d>f fdrop fdepth -> 0
```

Source: `tests/forth2012-test-suite/src/fp/ak-fp-test.fth`

## SEE ALSO

- [`d>f`](d_f.md)
- [`f>d`](f_d.md)
- [`fdepth`](fdepth.md)
