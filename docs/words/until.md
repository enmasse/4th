# UNTIL

## NAME

`UNTIL`

## SYNOPSIS

`UNTIL`

## DESCRIPTION

End a BEGIN...UNTIL loop

## FLAGS

- Module: `(core)`
- Immediate: `True`
- Async: `False`

## EXAMPLES

```forth
: GI4 BEGIN DUP 1+ DUP 5 > UNTIL ; ->
```

Source: `tests/forth-tests/core.fr`

```forth
: LT27 {: A :} 0 BEGIN A 1- TO A 3 + A 0= UNTIL ; ->
```

Source: `tests/forth-tests/localstest.fth`

```forth
: PT5  ( N1 -- )
PT4 !
BEGIN
-1 PT4 +!
PT4 @ 4 > 0= ?REPEAT \ Back TO BEGIN if FALSE
111
PT4 @ 3 > 0= ?REPEAT
222
PT4 @ 2 > 0= ?REPEAT
333
PT4 @ 1 =
UNTIL
; ->
```

Source: `tests/forth-tests/toolstest.fth`

## SEE ALSO

- [`!`](_.md)
- [`+`](_.md)
- [`+!`](__.md)
- [`0=`](0_.md)
- [`1+`](1_.md)
- [`1-`](1_.md)
- [`:`](_.md)
- [`;`](_.md)
- [`=`](_.md)
- [`>`](_.md)
- [`@`](_.md)
- [`BEGIN`](begin.md)
