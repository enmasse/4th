# Recommended Additional Tests for ANS Forth Compliance

## 1. FLOOR Integration Tests

### Test FLOOR in Chained Float Operations
```csharp
[Fact]
public async Task FLOOR_InChainedOperations()
{
    var forth = new ForthInterpreter();
    
    // FLOOR result used in multiplication (paranoia.4th pattern)
    Assert.True(await forth.EvalAsync("FVARIABLE X"));
    Assert.True(await forth.EvalAsync("3.7d X F!"));
    Assert.True(await forth.EvalAsync("X F@ FLOOR 2.0d F*"));
    Assert.Equal(2, forth.Stack.Count);
    Assert.IsType<double>(forth.Stack[1]);
    Assert.Equal(6.0, (double)forth.Stack[1], 10); // floor(3.7) * 2 = 3.0 * 2 = 6.0
    
    // Complex expression from paranoia.4th
    Assert.True(await forth.EvalAsync("0.5d 3.7d F+ FLOOR 2.0d F*"));
    Assert.Equal(4, forth.Stack.Count);
    Assert.Equal(8.0, (double)forth.Stack[3], 10); // floor(0.5 + 3.7) * 2 = floor(4.2) * 2 = 4.0 * 2 = 8.0
}
```

## 2. FLOATS Memory Allocation Pattern

### Test FLOATS in Memory Calculations
```csharp
[Fact]
public async Task FLOATS_InMemoryAllocation()
{
    var forth = new ForthInterpreter();
    
    // Typical pattern: allocate array of N floats
    Assert.True(await forth.EvalAsync("10 FLOATS ALLOCATE"));
    Assert.Equal(2, forth.Stack.Count);
    var addr = (long)forth.Stack[0];
    var ior = (long)forth.Stack[1];
    Assert.Equal(0L, ior); // success
    Assert.True(addr > 0);
    
    // Calculate offset for 5th float
    Assert.True(await forth.EvalAsync("5 FLOATS"));
    Assert.Single(forth.Stack);
    Assert.Equal(40L, (long)forth.Stack[0]); // 5 * 8 = 40 bytes
}
```

## 3. F~ Edge Cases

### Test F~ with Very Small/Large Numbers
```csharp
[Fact]
public async Task FTilde_EdgeCases()
{
    var forth = new ForthInterpreter();
    
    // Very small numbers with absolute tolerance
    Assert.True(await forth.EvalAsync("1.0E-10 1.0001E-10 1.0E-9 F~"));
    Assert.Single(forth.Stack);
    Assert.Equal(-1L, (long)forth.Stack[0]); // true: difference < tolerance
    
    // Very large numbers with relative tolerance
    Assert.True(await forth.EvalAsync("1.0E10 1.001E10 -0.01 F~"));
    Assert.Equal(2, forth.Stack.Count);
    Assert.Equal(-1L, (long)forth.Stack[1]); // true: 0.1% < 1%
    
    // Zero with relative tolerance (avoid division by zero)
    Assert.True(await forth.EvalAsync("0.0d 0.0d -0.01 F~"));
    Assert.Equal(3, forth.Stack.Count);
    Assert.Equal(-1L, (long)forth.Stack[2]); // true: both are zero
    
    // Near-zero with relative tolerance
    Assert.True(await forth.EvalAsync("1.0E-300 2.0E-300 -0.5 F~"));
    Assert.Equal(4, forth.Stack.Count);
    Assert.Equal(-1L, (long)forth.Stack[3]); // true: 50% difference, tolerance is 50%
}
```

## 4. FLN/FLOG Interchangeability

### Test FLN and FLOG in Real Calculations
```csharp
[Fact]
public async Task FLN_FLOG_Interchangeable()
{
    var forth = new ForthInterpreter();
    
    // Use both in same calculation (paranoia.4th pattern)
    Assert.True(await forth.EvalAsync("240.0d U1 F!"));
    Assert.True(await forth.EvalAsync("0.001d X F!"));
    Assert.True(await forth.EvalAsync("2.0d Radix F!"));
    
    // Calculate precision using FLN
    Assert.True(await forth.EvalAsync("240.0d 0.001d FLN F* 2.0d FLN F/ FNEGATE"));
    var stack1Count = forth.Stack.Count;
    var result1 = (double)forth.Stack[^1];
    
    // Calculate same using FLOG
    Assert.True(await forth.EvalAsync("240.0d 0.001d FLOG F* 2.0d FLOG F/ FNEGATE"));
    var stack2Count = forth.Stack.Count;
    var result2 = (double)forth.Stack[^1];
    
    Assert.Equal(stack1Count + 1, stack2Count);
    Assert.Equal(result1, result2, 15); // Should be identical
}
```

## 5. Combined Compliance Test

### Test All Fixes Together (Real paranoia.4th Fragment)
```csharp
[Fact]
public async Task ANS_Compliance_RealParanoiaFragment()
{
    var forth = new ForthInterpreter();
    
    // Setup variables from paranoia.4th
    Assert.True(await forth.EvalAsync(@"
        FVARIABLE Half    0.5d Half F!
        FVARIABLE One     1.0d One F!
        FVARIABLE Radix   2.0d Radix F!
        FVARIABLE U1      0.001d U1 F!
        FVARIABLE X       0.0d X F!
        FVARIABLE Y       0.0d Y F!
    "));
    
    // Test FLOOR returning float (from paranoia.4th line ~2550)
    Assert.True(await forth.EvalAsync(@"
        Half F@ X F@ F+ FLOOR Radix F@ F* X F@ F+ X F!
    "));
    // Should complete without error
    
    // Test FLOATS for precision detection (from paranoia.4th line ~45)
    Assert.True(await forth.EvalAsync("1 FLOATS"));
    Assert.Single(forth.Stack);
    var floatSize = (long)forth.Stack[0];
    Assert.Equal(8L, floatSize); // Should detect double precision
    
    // Test FLN in precision calculation (from paranoia.4th line ~2580)
    Assert.True(await forth.EvalAsync(@"
        240.0d U1 F@ FLN F* Radix F@ FLN F/ FNEGATE X F!
    "));
    Assert.Equal(2, forth.Stack.Count);
    var precision = (double)forth.Stack[1];
    Assert.True(precision > 0); // Should calculate positive precision
    
    // Test F~ with zero tolerance (from paranoia.4th)
    Assert.True(await forth.EvalAsync(@"
        One F@ One F@ 0.0d F~
    "));
    Assert.Equal(3, forth.Stack.Count);
    Assert.Equal(-1L, (long)forth.Stack[2]); // Should be exactly equal
}
```

## 6. Regression Tests

### Test That Old Behavior Is Not Broken
```csharp
[Fact]
public async Task Regression_NonFloatOperationsStillWork()
{
    var forth = new ForthInterpreter();
    
    // Ensure integer FLOOR-like behavior via F>S still works
    Assert.True(await forth.EvalAsync("3.7d F>S"));
    Assert.Single(forth.Stack);
    Assert.IsType<long>(forth.Stack[0]);
    Assert.Equal(3L, (long)forth.Stack[0]);
    
    // Ensure CELLS (not FLOATS) still returns n unchanged for 1-cell system
    // (This is different from FLOATS which returns n*8)
    Assert.True(await forth.EvalAsync("1 CELLS"));
    Assert.Equal(2, forth.Stack.Count);
    // CELLS returns cell size (8 bytes on 64-bit system)
    Assert.Equal(8L, (long)forth.Stack[1]);
}
```

## Priority Recommendation

I recommend adding tests **#1 (FLOOR in chained operations)** and **#3 (F~ edge cases)** as these are the most likely to catch real-world issues that the current tests might miss.

The `ANS_Compliance_RealParanoiaFragment` test (#5) would be excellent for confidence but might be overkill if you're already planning to run the full paranoia.4th test suite.
