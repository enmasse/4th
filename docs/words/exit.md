# EXIT

## NAME

`EXIT`

## SYNOPSIS

`EXIT`

## DESCRIPTION

Exit from current word

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
: GD6  ( PAT: T{0 0},{0 0}{1 0}{1 1},{0 0}{1 0}{1 1}{2 0}{2 1}{2 2} )
0 SWAP 0 DO
I 1+ 0 DO I J + 3 = IF I UNLOOP I UNLOOP EXIT THEN 1+ LOOP
LOOP ; ->
```

Source: `tests/forth-tests/core.fr`

```forth
: AG0 701 BEGIN DUP 7 MOD 0= IF EXIT THEN 1+ AGAIN ; ->
```

Source: `tests/forth-tests/coreexttest.fth`

```forth
: UNS1 DUP 0 > IF 9 SWAP BEGIN 1+ DUP 3 > IF EXIT THEN REPEAT ; ->
```

Source: `tests/forth-tests/coreplustest.fth`

## SEE ALSO

- [`+`](_.md)
- [`0=`](0_.md)
- [`1+`](1_.md)
- [`:`](_.md)
- [`;`](_.md)
- [`=`](_.md)
- [`>`](_.md)
- [`AGAIN`](again.md)
- [`BEGIN`](begin.md)
- [`DO`](do.md)
- [`DUP`](dup.md)
- [`I`](i.md)
