# Intervals.Tools

This .NET package provides basic set of tools for working with generic intervals in the form series of (start, end, interval type) pairs. Supported operations are intersect, except, merge, invert, union etc. from the class **`IntervalSet`**.

`// TODO: Enhance with invert; slice; union(itv[]); complement(itv, itv); inRange(itv/limit)`

Intended for use cases, involving set of intervals.

This package is intended to simplify and improve usability and performance in use cases requiring often mutation (add, remove, update) and search operations over intervals.

## About

There aren't many packages available on [NuGet](https://www.nuget.org/), providing these functionalities with flexible and friendly enough programming API. My goal is to provide useful and rich library for convenient work with intervals. Hence, I created this enhanced one.

However, I didn't find one that really suited my needs so I created this enhanced one. I want to create a README template so amazing that it'll be the last one you ever need -- I think this is it.

## Usage

//TODO: Add more examples

### IntervalSet

IntervalSet is a generic collection providing basic functionalities over intervals of data.

Here is an example creating and filling an IntervalSet with intervals and applying intersection and exception over the collection:

```CSharp
var intervalSet = new IntervalSet<int>();

intervalSet.Add((2, 5)); // (2, 5) - if interval type is not specified, interval is open by default
intervalSet.Add((3, 8, IntervalType.Open)); // (3, 8)
intervalSet.Add((3, 8, IntervalType.Closed)); // [3, 8]
intervalSet.Add((11, 16, IntervalType.StartClosed)); // [11, 16)
intervalSet.Add((11, 14, IntervalType.EndClosed)); // (11, 14]

var intersectedIntervalSet1 = intervalSet
    .Intersect((8, 11, IntervalType.Closed)); // [8, 11]
Console.WriteLine("[{0}]", string.Join(", ", intersectedIntervalSet1));
// [[3, 8], [11, 16)]

var intersectedIntervalSet2 = intervalSet
    .Intersect((8, 11)); // (8, 11)
Console.WriteLine("[{0}]", string.Join(", ", intersectedIntervalSet2));
// []

var exceptedIntervalSet1 = intervalSet
    .Except((8, 11, IntervalType.Closed)); // [8, 11]
Console.WriteLine("[{0}]", string.Join(", ", exceptedIntervalSet1));
// [(2, 5), (3, 8), (11, 14]]

var exceptedIntervalSet2 = intervalSet
    .Except((8, 11)); // (8, 11)
Console.WriteLine("[{0}]", string.Join(", ", exceptedIntervalSet2));
// [(2, 5), (3, 8), (3, 8], [11, 16), (11, 14]]
```

You can also specify the type of intersection between the given interval and the interval set. By default the intersection type is **Any**, searching for any kind of intersection between the intervals:

```CSharp
var intervalSet = new IntervalSet<int>();

intervalSet.Add((2, 5));
intervalSet.Add((5, 10, IntervalType.StartClosed));
intervalSet.Add((3, 12, IntervalType.Closed));
intervalSet.Add((11, 16, IntervalType.StartClosed));

var intersectionInterval = (5, 11, IntervalType.Closed);
var intersectedIntervalSet1 = intervalSet
    .Intersect(intersectionInterval, IntersectionType.Any);
Console.WriteLine(
    "[{0}] has any intersection with {1}",
    intersectionInterval,
    string.Join(", ", intersectedIntervalSet1));
// [5, 11] has any intersection with [[3, 12], [5, 10), [11, 16)]
```

There are 2 more type of intersection - **Cover** where the given interval covers intervals and **Within** where the given one is within intervals.

Here is an example with an interval set intersected by an input covering interval. When the **Intersect** called, a new interval set is returned with the covered intervals by the input interval.

```CSharp
var intervalSet = new IntervalSet<int>
{
    (2, 5), // (2, 5)
    (5, 10, IntervalType.StartClosed), // [5, 10)
    (3, 12, IntervalType.Closed), // [3, 12]
    (11, 16, IntervalType.StartClosed) // [11, 16)
};

var intersectionInterval = (5, 11, IntervalType.Closed);

var coveredIntervalSet = intervalSet
    .Intersect(intersectionInterval, IntersectionType.Cover);
Console.WriteLine(
    "{0} covers [{1}]",
    intersectionInterval,
    string.Join(", ", coveredIntervalSet));
// [5, 11] covers [[5, 10)]
```

The following example is presenting intersection of interval set with an input interval that is within each intersected interval.

```CSharp
var intervalSet = new IntervalSet<int>
{
    (2, 5), // (2, 5)
    (5, 10, IntervalType.StartClosed), // [5, 10)
    (3, 12, IntervalType.Closed), // [3, 12]
    (11, 16, IntervalType.StartClosed) // [11, 16)
};

var intersectionInterval = (5, 11, IntervalType.Closed);

var coveringInterval = intervalSet
    .Intersect(intersectionInterval, IntersectionType.Within);
Console.WriteLine(
    "{0} is within [{1}]",
    intersectionInterval,
    string.Join(", ", coveringInterval));
// [5, 11] is within [[3, 12]]
```

You can also merge intervals. Calling **Merge** returns IntervalSet with the merged intervals.

```CSharp
var intervalSet = new IntervalSet<int>
{
    (2, 5), // (2, 5)
    (5, 10, IntervalType.StartClosed), // [5, 10)
    (12, 16, IntervalType.Closed), // [12, 16]
    (14, 26, IntervalType.StartClosed) // [14, 26)
};

var mergedIntervalSet = intervalSet.Merge();
Console.WriteLine("[{1}]", string.Join(", ", intersectedIntervalSet3));
// [(2, 10), [12, 26)]
```

## License

// TODO: add MIT license

## Donate

// TODO: enable GitHub Sponsorship
