# >NUMBER

## NAME

`>NUMBER` — accumulate digits per BASE, report remainder

## SYNOPSIS

`>NUMBER ( c-addr u acc start -- acc' remflag digits )`

## DESCRIPTION

>NUMBER ( c-addr u acc start -- acc' remflag digits ) - accumulate digits per BASE, report remainder

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
0 0 GN' 0' >NUMBER -> 0 0 GN-CONSUMED
```

Source: `tests/forth-tests/core.fr`

```forth
0 0 GN' 1' >NUMBER -> 1 0 GN-CONSUMED
```

Source: `tests/forth-tests/core.fr`

```forth
1 0 GN' 1' >NUMBER -> BASE @ 1+ 0 GN-CONSUMED
```

Source: `tests/forth-tests/core.fr`

## SEE ALSO

- [`1+`](1_.md)
- [`@`](_.md)
- [`BASE`](base.md)
