# F>D

## NAME

`F>D` — convert floating-point number to double-cell integer

## SYNOPSIS

`F>D ( r -- d )`

## DESCRIPTION

F>D ( r -- d ) - convert floating-point number to double-cell integer

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
0. d>f 0. d>f fdrop fdepth f>d -> 1 0.
```

Source: `tests/forth-tests/fp/ak-fp-test.fth`

```forth
0. d>f f>d -> 0.
```

Source: `tests/forth-tests/fp/ak-fp-test.fth`

## SEE ALSO

- [`d>f`](d_f.md)
- [`fdepth`](fdepth.md)
- [`fdrop`](fdrop.md)
