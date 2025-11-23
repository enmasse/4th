: ADD2RAW IL{ 
  ldarg.0 
  callvirt "Forth.Core.Interpreter.ForthInterpreter::Pop()" 
  call "Forth.Core.Interpreter.ForthInterpreter::ToLong(object)" 
  stloc.s 0 
  ldarg.0 
  callvirt "Forth.Core.Interpreter.ForthInterpreter::Pop()" 
  call "Forth.Core.Interpreter.ForthInterpreter::ToLong(object)" 
  ldloc.s 0 
  add 
  stloc.s 0 
  ldarg.0 
  ldloc.s 0 
  box System.Int64 
  callvirt "Forth.Core.Interpreter.ForthInterpreter::Push(object)" 
  ret 
}IL ;

: ADD2_FIXED IL{ 
  ldarg.0 
  callvirt "Forth.Core.Interpreter.ForthInterpreter::Pop()" 
  call "Forth.Core.Interpreter.ForthInterpreter::ToLong(object)" 
  stloc.0 
  ldarg.0 
  callvirt "Forth.Core.Interpreter.ForthInterpreter::Pop()" 
  call "Forth.Core.Interpreter.ForthInterpreter::ToLong(object)" 
  stloc.1 
  ldloc.1 
  ldloc.0 
  add 
  stloc.0 
  ldarg.0 
  ldloc.0 
  box System.Int64 
  callvirt "Forth.Core.Interpreter.ForthInterpreter::Push(object)" 
  ret 
}IL ;

: ADD2_SHORTVAR IL{ 
  ldarg.0 
  callvirt "Forth.Core.Interpreter.ForthInterpreter::Pop()" 
  call "Forth.Core.Interpreter.ForthInterpreter::ToLong(object)" 
  stloc.s 0 
  ldarg.0 
  callvirt "Forth.Core.Interpreter.ForthInterpreter::Pop()" 
  call "Forth.Core.Interpreter.ForthInterpreter::ToLong(object)" 
  stloc.s 1 
  ldloc.s 1 
  ldloc.s 0 
  add 
  stloc.s 0 
  ldarg.0 
  ldloc.s 0 
  box System.Int64 
  callvirt "Forth.Core.Interpreter.ForthInterpreter::Push(object)" 
  ret 
}IL ;

: ADD2_INLINEVAR IL{ 
  ldarg.0 
  callvirt "Forth.Core.Interpreter.ForthInterpreter::Pop()" 
  call "Forth.Core.Interpreter.ForthInterpreter::ToLong(object)" 
  stloc 0 
  ldarg.0 
  callvirt "Forth.Core.Interpreter.ForthInterpreter::Pop()" 
  call "Forth.Core.Interpreter.ForthInterpreter::ToLong(object)" 
  stloc 1 
  ldloc 1 
  ldloc 0 
  add 
  stloc 0 
  ldarg.0 
  ldloc 0 
  box System.Int64 
  callvirt "Forth.Core.Interpreter.ForthInterpreter::Push(object)" 
  ret 
}IL ;

: PUSH123RAW IL{ 
  ldarg.0 
  ldc.i4.s 123 
  conv.i8 
  box System.Int64 
  callvirt "Forth.Core.Interpreter.ForthInterpreter::Push(object)" 
  ret 
}IL ;

: POPPUSH IL{ 
  ldarg.0 
  call "Forth.Core.Interpreter.ForthInterpreter::Pop()" 
  stloc.s 0 
  ldarg.0 
  ldloc.s 0 
  call "Forth.Core.Interpreter.ForthInterpreter::Push(object)" 
  ret 
}IL ;

: POPPUSH_STACK IL{ 
  ldarg.1 
  call "Forth.Core.Interpreter.ForthStack::Pop()" 
  stloc.s 0 
  ldarg.1 
  ldloc.s 0 
  call "Forth.Core.Interpreter.ForthStack::Push(object)" 
  ret 
}IL ;

: INC1RAW IL{ 
  ldarg.0 
  callvirt "Forth.Core.Interpreter.ForthInterpreter::Pop()" 
  call "Forth.Core.Interpreter.ForthInterpreter::ToLong(object)" 
  ldc.i4.1 
  conv.i8 
  add 
  stloc.s 0 
  ldarg.0 
  ldloc.s 0 
  box System.Int64 
  callvirt "Forth.Core.Interpreter.ForthInterpreter::Push(object)" 
  ret 
}IL ;

: INCNZ IL{ 
  ldarg.0 
  callvirt "Forth.Core.Interpreter.ForthInterpreter::Pop()" 
  call "Forth.Core.Interpreter.ForthInterpreter::ToLong(object)" 
  stloc.s 0 
  ldloc.s 0 
  ldc.i4.0 
  conv.i8 
  ceq 
  brtrue.s SKIP 
  ldloc.s 0 
  ldc.i4.1 
  conv.i8 
  add 
  stloc.s 0 
  SKIP: 
  ldarg.0 
  ldloc.s 0 
  box System.Int64 
  callvirt "Forth.Core.Interpreter.ForthInterpreter::Push(object)" 
  ret 
}IL ;

: SUM1TON IL{ 
  ldarg.0 
  callvirt "Forth.Core.Interpreter.ForthInterpreter::Pop()" 
  call "Forth.Core.Interpreter.ForthInterpreter::ToLong(object)" 
  stloc.s 0 
  ldc.i4.0 
  conv.i8 
  stloc.s 1 
  LOOP: 
  ldloc.s 0 
  ldc.i4.0 
  conv.i8 
  ceq 
  brtrue.s END 
  ldloc.s 1 
  ldloc.s 0 
  add 
  stloc.s 1 
  ldloc.s 0 
  ldc.i4.1 
  conv.i8 
  sub 
  stloc.s 0 
  br.s LOOP 
  END: 
  ldarg.0 
  ldloc.s 1 
  box System.Int64 
  callvirt "Forth.Core.Interpreter.ForthInterpreter::Push(object)" 
  ret 
}IL ;

: NEGIFPOS IL{ 
  ldarg.0 
  callvirt "Forth.Core.Interpreter.ForthInterpreter::Pop()" 
  call "Forth.Core.Interpreter.ForthInterpreter::ToLong(object)" 
  stloc.s 0 
  ldloc.s 0 
  ldc.i4.0 
  conv.i8 
  cgt 
  brfalse.s DONE 
  ldloc.s 0 
  neg 
  stloc.s 0 
  DONE: 
  ldarg.0 
  ldloc.s 0 
  box System.Int64 
  callvirt "Forth.Core.Interpreter.ForthInterpreter::Push(object)" 
  ret 
}IL ;

INCLUDE "../tester.fs"

\ Arithmetic
T{ 8 9 ADD2RAW -> 17 }T
T{ 3 4 ADD2_FIXED -> 7 }T
T{ 10 20 ADD2_SHORTVAR -> 30 }T
T{ 7 8 ADD2_INLINEVAR -> 15 }T
T{ PUSH123RAW -> 123 }T

\ Roundtrip pop/push
T{ 42 POPPUSH -> 42 }T
T{ 99 POPPUSH_STACK -> 99 }T

\ Increment
T{ 41 INC1RAW -> 42 }T

\ Conditional increment if non-zero
T{ 0 INCNZ -> 0 }T
T{ 5 INCNZ -> 6 }T

\ Loop sum
T{ 5 SUM1TON -> 15 }T

\ Conditional negate
T{ 5 NEGIFPOS -> -5 }T
T{ 0 NEGIFPOS -> 0 }T
T{ -3 NEGIFPOS -> -3 }T
