# AND

## NAME

`AND`

## SYNOPSIS

`AND ( a b -- a&b )`

## DESCRIPTION

Bitwise AND of two numbers ( a b -- a&b )

## FLAGS

- Module: `(core)`
- Immediate: `False`
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
0 0 AND -> 0
```

Source: `tests/forth-tests/core.fr`

```forth
0 1 AND -> 0
```

Source: `tests/forth-tests/core.fr`

## SEE ALSO

- [`,`](_.md)
- [`of`](of.md)
