using BenchmarkDotNet.Running;
using Intervals.Tools.Playground;

BenchmarkRunner.Run<IntervalCollectionsInitializationBenchmarks>();
BenchmarkRunner.Run<IntervalCollectionsBenchmarks>();
