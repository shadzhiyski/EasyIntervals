using BenchmarkDotNet.Running;
using EasyIntervals.Playground;

BenchmarkRunner.Run<IntervalCollectionsInitializationBenchmarks>();
BenchmarkRunner.Run<IntervalCollectionsBenchmarks>();
