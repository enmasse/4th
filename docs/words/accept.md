# ACCEPT

## NAME

`ACCEPT` — read line excluding CR/LF terminators

## SYNOPSIS

`ACCEPT ( addr u -- u )`

## DESCRIPTION

ACCEPT ( addr u -- u ) - read line excluding CR/LF terminators

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

- [`ALLOT`](allot.md)
- [`CREATE`](create.md)
