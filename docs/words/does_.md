# DOES>

## NAME

`DOES>` — begin definition of runtime behavior for last CREATE

## SYNOPSIS

`DOES>`

## DESCRIPTION

DOES> - begin definition of runtime behavior for last CREATE

## FLAGS

- Module: `(core)`
- Immediate: `True`
- Async: `False`

## EXAMPLES

```forth
: DOES1 DOES> @ 1 + ; ->
```

Source: `tests/forth-tests/core.fr`

```forth
: DOES2 DOES> @ 2 + ; ->
```

Source: `tests/forth-tests/core.fr`

```forth
: WEIRD: CREATE DOES> 1 + DOES> 2 + ; ->
```

Source: `tests/forth-tests/core.fr`

## SEE ALSO

- [`+`](_.md)
- [`:`](_.md)
- [`;`](_.md)
- [`@`](_.md)
- [`CREATE`](create.md)
