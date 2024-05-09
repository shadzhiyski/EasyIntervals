using BenchmarkDotNet.Running;
using NeatIntervals.Playground;

BenchmarkRunner.Run<IntervalCollectionsInitializationBenchmarks>();
BenchmarkRunner.Run<IntervalCollectionsBenchmarks>();
