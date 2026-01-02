# DU<

## NAME

`DU<` — true if ud1 < ud2 unsigned

## SYNOPSIS

`DU< ( ud1 ud2 -- flag )`

## DESCRIPTION

DU< ( ud1 ud2 -- flag ) - true if ud1 < ud2 unsigned

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
1.  1. DU< -> FALSE
```

Source: `tests/forth-tests/doubletest.fth`

```forth
1. -1. DU< -> TRUE
```

Source: `tests/forth-tests/doubletest.fth`

```forth
-1.  1. DU< -> FALSE
```

Source: `tests/forth-tests/doubletest.fth`

## SEE ALSO

- (none yet)
