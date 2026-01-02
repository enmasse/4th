# OF

## NAME

`OF` — compare and branch ( sel test -- sel | )

## SYNOPSIS

`OF ( sel test -- sel | )`

## DESCRIPTION

OF - compare and branch ( sel test -- sel | )

## FLAGS

- Module: `(core)`
- Immediate: `True`
- Async: `False`

## EXAMPLES

```forth
, and extensions for the handling of floating point tests.
\ Code for testing equality of floating point values comes
\ from ftester.fs written by David N. Williams, based on the idea of
\ approximate equality in Dirk Zoller's float.4th.
\ Further revisions were provided by Anton Ertl, including the ability
\ to handle either integrated or separate floating point stacks.
\ Revision history and possibly newer versions can be found at
\ http://www.complang.tuwien.ac.at/cvsweb/cgi-bin/cvsweb/gforth/test/ttester.fs
\ Explanatory material and minor reformatting (no code changes) by
\ C. G. Montgomery March 2009, with helpful comments from David Williams
\ and Krishna Myneni.
\ Usage:
\ The basic usage takes the form  T{ <code> -> <expected stack>
```

Source: `tests/ttester.4th`

```forth
: CS6 CASE 1 OF ENDOF 2 ENDCASE ; 1 CS6 ->
```

Source: `tests/forth-tests/coreexttest.fth`

```forth
: CS7 CASE 3 OF ENDOF 2 ENDCASE ; 1 CS7 -> 1
```

Source: `tests/forth-tests/coreexttest.fth`

## SEE ALSO

- [`,`](_.md)
- [`:`](_.md)
- [`;`](_.md)
- [`and`](and.md)
- [`CASE`](case.md)
- [`ENDCASE`](endcase.md)
- [`ENDOF`](endof.md)
