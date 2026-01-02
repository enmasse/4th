# CREATE

## NAME

`CREATE` — create a new data-definition word

## SYNOPSIS

`CREATE`

## DESCRIPTION

CREATE <name> - create a new data-definition word

## FLAGS

- Module: `(core)`
- Immediate: `True`
- Async: `False`

## EXAMPLES

```forth
CREATE CR1 ->
```

Source: `tests/forth-tests/core.fr`

```forth
: WEIRD: CREATE DOES> 1 + DOES> 2 + ; ->
```

Source: `tests/forth-tests/core.fr`

```forth
CREATE IW5 456 , IMMEDIATE ->
```

Source: `tests/forth-tests/coreplustest.fth`

## SEE ALSO

- [`+`](_.md)
- [`,`](_.md)
- [`:`](_.md)
- [`;`](_.md)
- [`DOES>`](does_.md)
- [`IMMEDIATE`](immediate.md)
