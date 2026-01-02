# INCLUDE

## NAME

`INCLUDE`

## SYNOPSIS

`INCLUDE`

## DESCRIPTION

INCLUDE <filename> interpret file contents

## FLAGS

- Module: `(core)`
- Immediate: `True`
- Async: `False`

## EXAMPLES

```forth
0
S" required-helper1.fth" REQUIRED
REQUIRE required-helper1.fth
INCLUDE required-helper1.fth
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

Source: `tests/forth-tests/filetest.fth`

```forth
0
S" required-helper1.fth" REQUIRED
REQUIRE required-helper1.fth
INCLUDE required-helper1.fth
-> 2
```

Source: `tests/forth2012-test-suite-local/src/filetest.fth`

## SEE ALSO

- [`INCLUDED`](included.md)
