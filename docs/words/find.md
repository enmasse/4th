# FIND

## NAME

`FIND` — find word in dictionary

## SYNOPSIS

`FIND ( c-addr -- c-addr 0 | xt 1 | xt -1 )`

## DESCRIPTION

FIND ( c-addr -- c-addr 0 | xt 1 | xt -1 ) - find word in dictionary

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
GT1STRING FIND -> ' GT1 -1
```

Source: `tests/forth-tests/core.fr`

```forth
GT2STRING FIND -> ' GT2 1
```

Source: `tests/forth-tests/core.fr`

```forth
: MA? BL WORD FIND NIP 0<> ; ->
```

Source: `tests/forth-tests/coreexttest.fth`

## SEE ALSO

- [`'`](_.md)
- [`0<>`](0__.md)
- [`:`](_.md)
- [`;`](_.md)
- [`BL`](bl.md)
- [`NIP`](nip.md)
- [`WORD`](word.md)
