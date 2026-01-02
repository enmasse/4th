# NIP

## NAME

`NIP`

## SYNOPSIS

`NIP ( a b -- b )`

## DESCRIPTION

NIP ( a b -- b ) drop second item

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
1 2 NIP -> 2
```

Source: `tests/forth-tests/coreexttest.fth`

```forth
1 2 3 NIP -> 1 3
```

Source: `tests/forth-tests/coreexttest.fth`

```forth
: MA? BL WORD FIND NIP 0<> ; ->
```

Source: `tests/forth-tests/coreexttest.fth`

## SEE ALSO

- [`0<>`](0__.md)
- [`:`](_.md)
- [`;`](_.md)
- [`BL`](bl.md)
- [`FIND`](find.md)
- [`WORD`](word.md)
