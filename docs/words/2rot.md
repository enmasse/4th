# 2ROT

## NAME

`2ROT` — rotate three pairs

## SYNOPSIS

`2ROT ( x1 x2 x3 x4 x5 x6 -- x3 x4 x5 x6 x1 x2 )`

## DESCRIPTION

2ROT ( x1 x2 x3 x4 x5 x6 -- x3 x4 x5 x6 x1 x2 ) - rotate three pairs

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
1. 2. 3. 2ROT -> 2. 3. 1.
```

Source: `tests/forth-tests/doubletest.fth`

```forth
MAX-2INT MIN-2INT 1. 2ROT -> MIN-2INT 1. MAX-2INT
```

Source: `tests/forth-tests/doubletest.fth`

```forth
1. 2. 3. 2ROT -> 2. 3. 1.
```

Source: `tests/forth2012-test-suite/src/doubletest.fth`

## SEE ALSO

- (none yet)
