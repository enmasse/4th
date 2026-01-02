# INCLUDED

## NAME

`INCLUDED` — interpret file

## SYNOPSIS

`INCLUDED ( i*x c-addr u | string -- j*x )`

## DESCRIPTION

INCLUDED ( i*x c-addr u | string -- j*x ) - interpret file

## FLAGS

- Module: `(core)`
- Immediate: `True`
- Async: `False`

## EXAMPLES

```forth
0
INCLUDE required-helper2.fth
S" required-helper2.fth" REQUIRED
REQUIRE required-helper2.fth
S" required-helper2.fth" INCLUDED
-> 2
```

Source: `tests/forth-tests/filetest.fth`

```forth
0
INCLUDE required-helper2.fth
S" required-helper2.fth" REQUIRED
REQUIRE required-helper2.fth
S" required-helper2.fth" INCLUDED
-> 2
```

Source: `tests/forth2012-test-suite/src/filetest.fth`

```forth
0
INCLUDE required-helper2.fth
S" required-helper2.fth" REQUIRED
REQUIRE required-helper2.fth
S" required-helper2.fth" INCLUDED
-> 2
```

Source: `tests/forth2012-test-suite-local/src/filetest.fth`

## SEE ALSO

- [`INCLUDE`](include.md)
