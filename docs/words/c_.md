# C@

## NAME

`C@` — fetch low byte at address

## SYNOPSIS

`C@ ( addr -- byte )`

## DESCRIPTION

C@ ( addr -- byte ) - fetch low byte at address

## FLAGS

- Module: `(core)`
- Immediate: `False`
- Async: `False`

## EXAMPLES

```forth
1STC C@ 2NDC C@ -> 1 2
```

Source: `tests/forth-tests/core.fr`

```forth
1STC C@ 2NDC C@ -> 3 2
```

Source: `tests/forth-tests/core.fr`

```forth
1STC C@ 2NDC C@ -> 3 4
```

Source: `tests/forth-tests/core.fr`

## SEE ALSO

- (none yet)
