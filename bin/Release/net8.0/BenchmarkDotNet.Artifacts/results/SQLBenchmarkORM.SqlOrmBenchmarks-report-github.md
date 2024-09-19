```

BenchmarkDotNet v0.14.0, Windows 10 (10.0.19045.4894/22H2/2022Update)
11th Gen Intel Core i5-1135G7 2.40GHz, 1 CPU, 8 logical and 4 physical cores
.NET SDK 8.0.202
  [Host]     : .NET 8.0.3 (8.0.324.11423), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI
  DefaultJob : .NET 8.0.3 (8.0.324.11423), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI


```
| Method             | Mean     | Error    | StdDev   |
|------------------- |---------:|---------:|---------:|
| OrmQuerySplitQuery |  9.092 s | 0.1623 s | 0.1518 s |
| OrmQuery           |  9.432 s | 0.1820 s | 0.2491 s |
| OrmLinQQuery       | 10.886 s | 0.2163 s | 0.5142 s |
| OrmQueryWithRawSql | 10.571 s | 0.2260 s | 0.6485 s |
