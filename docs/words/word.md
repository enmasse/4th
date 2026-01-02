# WORD

## NAME

`WORD` — parse word delimited by char

## SYNOPSIS

`WORD ( char "<chars>ccc<char>" -- c-addr )`

## DESCRIPTION

WORD ( char "<chars>ccc<char>" -- c-addr ) - parse word delimited by char

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
: MA? BL WORD FIND NIP 0<> ; ->
```

Source: `tests/forth-tests/coreexttest.fth`

```forth
: IMM? BL WORD FIND NIP ; IMM? .( -> 1
```

Source: `tests/forth-tests/coreexttest.fth`

```forth
: LOCAL BL WORD COUNT (LOCAL) ; IMMEDIATE ->
```

Source: `tests/forth-tests/localstest.fth`

## SEE ALSO

- [`.(`](__.md)
- [`0<>`](0__.md)
- [`:`](_.md)
- [`;`](_.md)
- [`BL`](bl.md)
- [`COUNT`](count.md)
- [`FIND`](find.md)
- [`IMMEDIATE`](immediate.md)
- [`NIP`](nip.md)
