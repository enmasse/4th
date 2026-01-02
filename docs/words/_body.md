# >BODY

## NAME

`>BODY` — get data-field address for CREATEd word or passthrough addr

## SYNOPSIS

`>BODY ( xt|addr -- a-addr )`

## DESCRIPTION

>BODY ( xt|addr -- a-addr ) - get data-field address for CREATEd word or passthrough addr

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
' CR1 >BODY -> HERE
```

Source: `tests/forth-tests/core.fr`

```forth
' W1 >BODY -> HERE
```

Source: `tests/forth-tests/core.fr`

```forth
CREATE 2K 3 , 2K , MAKE-2CONST 2K -> ' 2K >BODY 3
```

Source: `tests/forth-tests/coreplustest.fth`

## SEE ALSO

- [`'`](_.md)
- [`,`](_.md)
- [`CREATE`](create.md)
- [`HERE`](here.md)
