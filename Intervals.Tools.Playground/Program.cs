using BenchmarkDotNet.Running;
using Intervals.Tools.Playground;

var result = BenchmarkRunner.Run<IntervalCollectionsBenchmarks>();
System.Console.WriteLine(result);