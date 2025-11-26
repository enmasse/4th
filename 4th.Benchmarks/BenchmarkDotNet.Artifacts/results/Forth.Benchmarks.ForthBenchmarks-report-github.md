```

BenchmarkDotNet v0.15.6, Windows 11 (10.0.26200.7171)
Snapdragon X Plus - X1P42100 - Qualcomm Oryon CPU 3.24GHz, 1 CPU, 8 logical and 8 physical cores
.NET SDK 9.0.304
  [Host]     : .NET 9.0.11 (9.0.11, 9.0.1125.51716), Arm64 RyuJIT armv8.0-a
  DefaultJob : .NET 9.0.11 (9.0.11, 9.0.1125.51716), Arm64 RyuJIT armv8.0-a


```
| Method                | Mean        | Error     | StdDev    | Gen0    | Gen1   | Allocated |
|---------------------- |------------:|----------:|----------:|--------:|-------:|----------:|
| StackPushPop_Long     | 13,382.0 ns |  30.66 ns |  27.18 ns | 22.9492 |      - |   96000 B |
| StackPushPop_String   | 13,367.5 ns |  98.81 ns |  87.59 ns | 16.8304 |      - |   70400 B |
| ArithmeticOperations  | 43,211.4 ns | 326.78 ns | 305.67 ns | 68.8477 |      - |  288000 B |
| EvalSimpleArithmetic  |    386.9 ns |   5.56 ns |   6.62 ns |  0.1631 | 0.0019 |     960 B |
| EvalLoop              | 61,340.5 ns | 576.03 ns | 510.64 ns | 46.8750 | 1.2207 |  196480 B |
| EvalTypeString        |  1,984.6 ns |   6.21 ns |   5.81 ns |  0.4158 |      - |    1744 B |
| EvalTypeCountedString |          NA |        NA |        NA |      NA |     NA |        NA |

Benchmarks with issues:
  ForthBenchmarks.EvalTypeCountedString: DefaultJob
