# C@

## NAME

`C@` — fetch low byte at address

## SYNOPSIS

`C@ ( addr -- byte )`

## DESCRIPTION

C@ ( addr -- byte ) - fetch low byte at address

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
S" hello" ADD-INPUT-LINE
CREATE B 16 ALLOT
B 10 ACCEPT
-> 5
B C@ B 1 + C@ B 2 + C@ B 3 + C@ B 4 + C@
-> 104 101 108 108 111
```

Source: `tests/forth/accept-tests.tester.4th`

```forth
S" hello\rworld" ADD-INPUT-LINE
CREATE B 16 ALLOT
B 10 ACCEPT
-> 5
B C@ B 1 + C@ B 2 + C@ B 3 + C@ B 4 + C@
-> 104 101 108 108 111
```

Source: `tests/forth/accept-tests.tester.4th`

```forth
S" hello\nworld" ADD-INPUT-LINE
CREATE B 16 ALLOT
B 10 ACCEPT
-> 5
B C@ B 1 + C@ B 2 + C@ B 3 + C@ B 4 + C@
-> 104 101 108 108 111
```

Source: `tests/forth/accept-tests.tester.4th`

## SEE ALSO

- [`+`](_.md)
- [`ACCEPT`](accept.md)
- [`ALLOT`](allot.md)
- [`CREATE`](create.md)
