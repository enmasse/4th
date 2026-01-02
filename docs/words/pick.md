# PICK

## NAME

`PICK`

## SYNOPSIS

`PICK ( n -- )`

## DESCRIPTION

Copy Nth item from top ( n -- ) 
PICK expects index n and pushes the item at that depth

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
RO5 2 PICK -> 100 200 300 400 500 300
```

Source: `tests/forth-tests/coreexttest.fth`

```forth
RO5 1 PICK -> RO5 OVER
```

Source: `tests/forth-tests/coreexttest.fth`

```forth
RO5 0 PICK -> RO5 DUP
```

Source: `tests/forth-tests/coreexttest.fth`

## SEE ALSO

- [`DUP`](dup.md)
- [`OVER`](over.md)
