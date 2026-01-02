# BASE

## NAME

`BASE`

## SYNOPSIS

`BASE`

## DESCRIPTION

Push address of BASE variable

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
1 0 GN' 1' >NUMBER -> BASE @ 1+ 0 GN-CONSUMED
```

Source: `tests/forth-tests/core.fr`

```forth
BASE @ HEX BASE @ DECIMAL BASE @ - SWAP BASE ! -> 6
```

Source: `tests/forth-tests/coreexttest.fth`

```forth
BASE @ OLD-BASE @ = -> <TRUE>
```

Source: `tests/forth-tests/coreplustest.fth`

## SEE ALSO

- [`!`](_.md)
- [`-`](_.md)
- [`1+`](1_.md)
- [`=`](_.md)
- [`>NUMBER`](_number.md)
- [`@`](_.md)
- [`DECIMAL`](decimal.md)
- [`HEX`](hex.md)
- [`SWAP`](swap.md)
