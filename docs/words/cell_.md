# CELL+

## NAME

`CELL+` — add cell size to address

## SYNOPSIS

`CELL+ ( addr -- addr+1 )`

## DESCRIPTION

CELL+ ( addr -- addr+1 ) - add cell size to address

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
1ST CELL+ -> 2ND
```

Source: `tests/forth-tests/core.fr`

```forth
3 A-ADDR CELL+ C!  A-ADDR CELL+ C@ -> 3
```

Source: `tests/forth-tests/core.fr`

```forth
1234 A-ADDR CELL+ !  A-ADDR CELL+ @ -> 1234
```

Source: `tests/forth-tests/core.fr`

## SEE ALSO

- [`!`](_.md)
- [`@`](_.md)
- [`C!`](c_.md)
- [`C@`](c_.md)
