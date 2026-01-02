# CREATE

## NAME

`CREATE` — create a new data-definition word

## SYNOPSIS

`CREATE`

## DESCRIPTION

CREATE <name> - create a new data-definition word

## FLAGS

- Module: `(core)`
- Immediate: `True`
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
- [`ALLOT`](allot.md)
