# ALLOT

## NAME

`ALLOT` — reserve u cells in dictionary

## SYNOPSIS

`ALLOT ( u -- )`

## DESCRIPTION

ALLOT ( u -- ) - reserve u cells in dictionary

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
S" hello" ADD-INPUT-LINE
CREATE B 16 ALLOT
B 10 ACCEPT
```

Source: `tests/forth/accept-tests.4th`

```forth
S" hello\rworld" ADD-INPUT-LINE
CREATE B 16 ALLOT
B 10 ACCEPT
```

Source: `tests/forth/accept-tests.4th`

```forth
S" hello\nworld" ADD-INPUT-LINE
CREATE B 16 ALLOT
B 10 ACCEPT
```

Source: `tests/forth/accept-tests.4th`

## SEE ALSO

- [`ACCEPT`](accept.md)
- [`CREATE`](create.md)
