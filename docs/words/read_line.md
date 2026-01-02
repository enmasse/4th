# READ-LINE

## NAME

`READ-LINE` — read a line excluding CR/LF terminators

## SYNOPSIS

`READ-LINE ( c-addr u -- actual )`

## DESCRIPTION

READ-LINE ( c-addr u -- actual ) - read a line excluding CR/LF terminators

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
S" HELLO" ADD-INPUT-LINE
CREATE B 16 ALLOT
B 10 READ-LINE DROP
B C@ B 1 + C@ B 2 + C@ B 3 + C@ B 4 + C@
-> 72 69 76 76 79
```

Source: `tests/forth/add-input-line-tests.tester.4th`

```forth
CREATE C 16 ALLOT
5 C !
72 C 1 + C! 69 C 2 + C! 76 C 3 + C! 76 C 4 + C! 79 C 5 + C!
C ADD-INPUT-LINE
CREATE B 16 ALLOT
B 10 READ-LINE DROP
B C@ B 1 + C@ B 2 + C@ B 3 + C@ B 4 + C@
-> 72 69 76 76 79
```

Source: `tests/forth/add-input-line-tests.tester.4th`

```forth
CREATE D 16 ALLOT
72 D C! 69 D 1 + C! 76 D 2 + C! 76 D 3 + C! 79 D 4 + C!
D 5 ADD-INPUT-LINE
CREATE B 16 ALLOT
B 10 READ-LINE DROP
B C@ B 1 + C@ B 2 + C@ B 3 + C@ B 4 + C@
-> 72 69 76 76 79
```

Source: `tests/forth/add-input-line-tests.tester.4th`

## SEE ALSO

- [`!`](_.md)
- [`+`](_.md)
- [`ALLOT`](allot.md)
- [`C!`](c_.md)
- [`C@`](c_.md)
- [`CREATE`](create.md)
- [`DROP`](drop.md)
