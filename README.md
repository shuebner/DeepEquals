# DeepEquals
Creates a dynamically compiled DeepEquals function that does not use slow reflection at call time

Usage:
```
private static readonly Func<MyClass, MyClass, bool> DeepEqualsFunc = DeepEquals.FromProperties<MyClass>()
...
public override bool Equals(object obj)
{
    return obj is MyClass other &&
        DeepEqualsFunc(this, other);
}
```

Performance:
```
|                    Method |     Mean |     Error |    StdDev |
|-------------------------- |---------:|----------:|----------:|
|   HandWrittenEqualsSingle | 2.386 ns | 0.0056 ns | 0.0053 ns |
|     GeneratedEqualsSingle | 2.861 ns | 0.0110 ns | 0.0098 ns |
| HandWrittenEqualsMultiple | 6.952 ns | 0.0878 ns | 0.0685 ns |
|   GeneratedEqualsMultiple | 7.506 ns | 0.0290 ns | 0.0271 ns |
```
