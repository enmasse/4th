# AHEAD

## NAME

`AHEAD` — compile unconditional forward branch

## SYNOPSIS

`AHEAD`

## DESCRIPTION

AHEAD - compile unconditional forward branch

## FLAGS

- Module: `(core)`
- Immediate: `True`
- Async: `False`

## EXAMPLES

```forth
: PT1 AHEAD 1111 2222 THEN 3333 ; ->
```

Source: `tests/forth-tests/toolstest.fth`

```forth
: PT8
>R
AHEAD 111
BEGIN 222
[1CS-ROLL]
THEN
333
R> 1- >R
R@ 0<
UNTIL
R> DROP
; ->
```

Source: `tests/forth-tests/toolstest.fth`

```forth
: PT1 AHEAD 1111 2222 THEN 3333 ; ->
```

Source: `tests/forth2012-test-suite/src/toolstest.fth`

## SEE ALSO

- [`0<`](0_.md)
- [`1-`](1_.md)
- [`:`](_.md)
- [`;`](_.md)
- [`>R`](_r.md)
- [`BEGIN`](begin.md)
- [`DROP`](drop.md)
- [`R>`](r_.md)
- [`R@`](r_.md)
- [`THEN`](then.md)
- [`UNTIL`](until.md)
