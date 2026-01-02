# AGAIN

## NAME

`AGAIN` — end a BEGIN...AGAIN infinite loop

## SYNOPSIS

`AGAIN`

## DESCRIPTION

AGAIN - end a BEGIN...AGAIN infinite loop

## FLAGS

- Module: `(core)`
- Immediate: `True`
- Async: `False`

## EXAMPLES

```forth
: AG0 701 BEGIN DUP 7 MOD 0= IF EXIT THEN 1+ AGAIN ; ->
```

Source: `tests/forth-tests/coreexttest.fth`

```forth
: AG0 701 BEGIN DUP 7 MOD 0= IF EXIT THEN 1+ AGAIN ; ->
```

Source: `tests/forth2012-test-suite/src/coreexttest.fth`

```forth
: AG0 701 BEGIN DUP 7 MOD 0= IF EXIT THEN 1+ AGAIN ; ->
```

Source: `tests/forth2012-test-suite-local/src/coreexttest.fth`

## SEE ALSO

- [`0=`](0_.md)
- [`1+`](1_.md)
- [`:`](_.md)
- [`;`](_.md)
- [`BEGIN`](begin.md)
- [`DUP`](dup.md)
- [`EXIT`](exit.md)
- [`IF`](if.md)
- [`MOD`](mod.md)
- [`THEN`](then.md)
