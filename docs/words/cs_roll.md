# CS-ROLL

## NAME

`CS-ROLL` — rotate top u+1 control-flow stack items

## SYNOPSIS

`CS-ROLL ( u -- )`

## DESCRIPTION

CS-ROLL ( u -- ) - rotate top u+1 control-flow stack items

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
: ?DONE POSTPONE IF 1 CS-ROLL ; IMMEDIATE ->
```

Source: `tests/forth-tests/toolstest.fth`

```forth
: ?DONE POSTPONE IF 1 CS-ROLL ; IMMEDIATE ->
```

Source: `tests/forth2012-test-suite-local/src/toolstest.fth`

```forth
: ?DONE POSTPONE IF 1 CS-ROLL ; IMMEDIATE ->
```

Source: `tests/forth2012-test-suite/src/toolstest.fth`

## SEE ALSO

- [`:`](_.md)
- [`;`](_.md)
- [`IF`](if.md)
- [`IMMEDIATE`](immediate.md)
- [`POSTPONE`](postpone.md)
